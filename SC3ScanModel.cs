#define ZeroScanWriteOut        // Write computed zero-scan to CSV
#undef ZeroScanWriteOut
#define RotatedScanWriteOut     // Write rotated scans to indiv CSVs
#undef RotatedScanWriteOut
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace scanengine
{
    /// <summary>
    /// Scan Model Class.
    /// Holds the scan configuration, and computes the
    /// required start and end points for various specified angles.
    /// Implements INotifyPropertyChanged to allow databinding and updates.
    /// </summary>
    public class SC3ScanModel : INotifyPropertyChanged
    {
        #region Private Variables
        public event PropertyChangedEventHandler PropertyChanged;
        private decimal _XOrigin;
        private decimal _YOrigin;
        private decimal _XDelta;
        private decimal _YDelta;
        private decimal _RowSpacing;
        private decimal _LaserFrequency;
        private decimal _ScanVelocity;
        private decimal _ScanAcceleration;
        private int _ScanAngles;
        private int _PointsRequired;
        private int _RowsRequired;
        private int _PointsPerLine;
        private readonly decimal _OpticalXOrigin;
        private readonly decimal _OpticalYOrigin;

        private readonly List<decimal[]> _ScanCoordinates = new List<decimal[]>();
        private List<decimal[]> _ScanVelocities = new List<decimal[]>();
        private List<decimal[]> _ScanAccelerations = new List<decimal[]>();

        private List<List<decimal[]>> _RotatedCoordinates = new List<List<decimal[]>>();

        #endregion Private Variables

        #region Scan Model Properties
        public decimal XOrigin
        {
            get
            {
                return _XOrigin;
            }
            set
            {
                _XOrigin = value;
            }
        }
        public decimal YOrigin
        {
            get
            {
                return _YOrigin;
            }
            set
            {
                _YOrigin = value;
            }
        }
        public decimal XDelta
        {
            get
            {
                return _XDelta;
            }
            set
            {
                _XDelta = value;
            }
        }
        public decimal YDelta
        {
            get
            {
                return _YDelta;
            }
            set
            {
                _YDelta = value;
            }
        }
        public decimal RowSpacing
        {
            get
            {
                return _RowSpacing;
            }
            set
            {
                _RowSpacing = value;
            }
        }
        public decimal LaserFrequency
        {
            get
            {
                return _LaserFrequency;
            }
            set
            {
                _LaserFrequency = value;
            }
        }
        public decimal ScanVelocity
        {
            get
            {
                return _ScanVelocity;
            }
            set
            {
                _ScanVelocity = value;
                OnPropertyChanged();
            }
        }
        public decimal ScanAcceleration
        {
            get
            {
                return _ScanAcceleration;
            }
            set
            {
                _ScanAcceleration = value;
                OnPropertyChanged();
            }
        }
        public int ScanAngles
        {
            get
            {
                return _ScanAngles;
            }
            set
            {
                _ScanAngles = value;
                OnPropertyChanged();
            }
        }
        public int PointsRequired
        {
            get
            {
                return _PointsRequired;
            }
            set
            {
                _PointsRequired = value;
                OnPropertyChanged();
            }
        }
        public int RowsRequired
        {
            get
            {
                return _RowsRequired;
            }
            set
            {
                _RowsRequired = value;
            }
        }
        public int PointsPerLine
        {
            get
            {
                return _PointsPerLine;
            }
            set
            {
                _PointsPerLine = value;
                OnPropertyChanged();
            }
        }
        public List<decimal[]> ScanCoordinates
        {
            get
            {
                return _ScanCoordinates;
            }
            set
            {
                ScanCoordinates = value;
                OnPropertyChanged();
            }
        }
        public List<decimal[]> ScanVelocities
        {
            get
            {
                return _ScanVelocities;
            }
            set
            {
                _ScanVelocities = value;
                OnPropertyChanged();
            }
        }
        public List<decimal[]> ScanAccelerations
        {
            get
            {
                return _ScanAccelerations;
            }
            set
            {
                _ScanAccelerations = value;
                OnPropertyChanged();
            }
        }

        public List<List<decimal[]>> RotatedCoordinates
        {
            get
            {
                return _RotatedCoordinates;
            }
            set
            {
                _RotatedCoordinates = value;
                OnPropertyChanged();
            }
        }
        public decimal OpticalXOrigin
        {
            get
            {
                return _OpticalXOrigin;
            }
        }
        public decimal OpticalYOrigin
        {
            get
            {
                return _OpticalYOrigin;
            }
        }
        public List<List<decimal[]>> BoundingBoxScans;
        public List<int> BoundingPointsRequired;
        public List<int> BoundingRowsRequired;

        public decimal RunUp = 0.0m;
        #endregion Scan Model Properties

        private readonly double Deg2Rad = Math.PI / 180.0d;
        #region Constructor
        public SC3ScanModel()
        {
            // Initalize the origin and delta information
            XOrigin = 0.0m;
            YOrigin = 0.0m;
            XDelta = 0.0m;
            YDelta = 0.0m;
            // Optical axis centerline in stage coordinates
            // Stage: MLS203-1
            _OpticalXOrigin = 54.819m - 3.0m;                  // Correction factors for
            _OpticalYOrigin = 43.489m - 1.0m;                  // optical axis / stage center mismatch
            // Set scan velocity.
            // TODO: Support configurable velocity magnitudes
            ScanVelocity = 100.0m;
            ScanAcceleration = 1500.0m;
            // Laser frequency set to Helios lowest frequency
            // (20kHz) divided by /10 for pulse divider usage.
            // TODO: Support configurable laser frequencies.
            LaserFrequency = 20000.0m;
            BoundingBoxScans = new List<List<decimal[]>>();
        }
        #endregion Constructor

        #region Scan Calculation Helpers
        /// <summary>
        /// Calculates the records (points) per line, assumes that
        /// XDelta, ScanVelocity and LaserFrequency are set.
        /// </summary>
        public void CalculatePointsPerLine()
        {
            if (ScanVelocity != 0.0m)
            {
                PointsPerLine = (int)(((XDelta - RunUp)/ ScanVelocity) * LaserFrequency);
            }
            return;
        }
        /// <summary>
        /// Calculates the records (points) that make up a single
        /// angle scan.
        /// </summary>
        public void CalculatePointsRequired()
        {
            if (RowsRequired != 0)
            {
                PointsRequired = PointsPerLine * RowsRequired;
                ComputeZeroScan();
            }
            return;
        }
        /// <summary>
        /// Calculates the rows required for the scan based on 
        /// YDelta and RowSpacing. 
        /// </summary>
        public void CalculateRowsRequired()
        {
            if (RowSpacing == 0)
            {
                RowsRequired = 0;
            }
            else
            {
                RowsRequired = (int)Math.Ceiling(YDelta / RowSpacing);
            }
            OnPropertyChanged("RowsRequired");
            return;
        }

        public void ComputeZeroScan()
        {
            // Ignore the zero-row case
            if (RowsRequired != 0)
            {
                decimal _yOffset = 0.0m;
                ScanCoordinates.Clear();
                for (int _rows = 0; _rows <= RowsRequired; _rows++)
                {
                    // Calculate x/y origin/delta for each needed row
                    decimal[] _tmpDecimal = new decimal[4];
                    _yOffset = (decimal)_rows * RowSpacing;
                    _tmpDecimal[0] = XOrigin;
                    _tmpDecimal[1] = YOrigin + _yOffset;
                    _tmpDecimal[2] = XOrigin + XDelta;
                    _tmpDecimal[3] = _tmpDecimal[1];
                    ScanCoordinates.Add(_tmpDecimal);
                }
                RotatedCoordinates.Clear();
                ComputeRotatedScans();

#if ZeroScanWriteOut
                Console.WriteLine("Writing Scan Coordinates to CSV...");
                String _filename = "scantest-" + RowsRequired.ToString() + "rows.csv";
                String _lines = "";
                foreach(decimal[] coords in ScanCoordinates)
                {
                    String _outstring = coords[0].ToString() + "," +
                        coords[1].ToString() + "," + coords[2].ToString() +
                        "," + coords[3].ToString() + "\n";
                    _lines += _outstring;
                }
                File.WriteAllText(_filename, _lines);
                Console.WriteLine("File write completed. Filename: {0}", _filename);
#endif
            }
        }
        /// <summary>
        /// Figures out the transformed coordinates for the scans
        /// based on the initial scan coordinates. 
        /// </summary>
        public void ComputeRotatedScans()
        {
            // Compute spacing between scans.
            // _angleNum is a counter
            double _angleSpacing;
            if (ScanAngles == 0)
            {
                _angleSpacing = 180;
            }
            else
            {
                _angleSpacing = 180 / ScanAngles;
            }
            int _angleNum = 0;
            double _currentAngleRadians;
            decimal _sine;
            decimal _cosine;

            // Iterate through each required angle.
            int i = 0;
            if (ScanCoordinates.Count != 0)
            {

                while (i < 180)
                {
                    _currentAngleRadians = i * (Math.PI / 180.0d);
                    _cosine = (decimal)Math.Cos(_currentAngleRadians);
                    _sine = (decimal)Math.Sin(_currentAngleRadians);

                    List<decimal[]> _tempList = new List<decimal[]>();
                    for (int k = 0; k < ScanCoordinates.Count - 1; k++)
                    {
                        decimal[] _coords = ScanCoordinates[k];
                        decimal[] _tmpOut = new decimal[4];

                        // From Notebook
                        // Xr = (Xnr - Xo)*cos(a) + (Ynr - Yo)*sin(a) + Xo
                        // Yr = -1*(Xnr - Xo)*sin(a) (Ynr - Yo)*cos(a) + Yo
                        _tmpOut[0] = Math.Round((_coords[0] - OpticalXOrigin) * _cosine +
                                     ((_coords[1] - OpticalYOrigin) * _sine) + OpticalXOrigin, 3);
                        _tmpOut[1] = Math.Round((-(_coords[0] - OpticalXOrigin) * _sine) +
                                     ((_coords[1] - OpticalYOrigin) * _cosine) + OpticalYOrigin, 3);
                        if(_tmpOut[1] > 75.0m)
                        {
                            _tmpOut[1] = 75.00m;
                        }
                        _tmpOut[2] = Math.Round((_coords[2] - OpticalXOrigin) * _cosine +
                                     ((_coords[3] - OpticalYOrigin) * _sine) + OpticalXOrigin, 3);
                        _tmpOut[3] = Math.Round((-(_coords[2] - OpticalXOrigin) * _sine) +
                                     (_coords[3] - OpticalYOrigin) * _cosine + OpticalYOrigin, 3);
                        if (_tmpOut[3] > 75.0m)
                        {
                            _tmpOut[3] = 75.00m;
                        }
                        _tempList.Add(_tmpOut);
                    }
                    RotatedCoordinates.Add(_tempList);
#if RotatedScanWriteOut
                    Console.WriteLine("Writing Scan Coordinates to CSV...");
                    String _filename = "scantest-" + i.ToString("D3") +
                            "deg-" + RowsRequired.ToString() + "rows.csv";
                    String _lines = "";
                    var _currentAngleList = RotatedCoordinates[_angleNum];
                    foreach (decimal[] coords in _currentAngleList)
                    {
                        String _outstring = coords[0].ToString() + "," +
                            coords[1].ToString() + "," + coords[2].ToString() +
                            "," + coords[3].ToString() + "\n";
                        _lines += _outstring;
                    }
                    File.WriteAllText(_filename, _lines);
                    Console.WriteLine("File write completed. Filename: {0}", _filename);
#endif
                    i += (int)Math.Round(_angleSpacing);
                    _angleNum++;
                }
            }
        }

        public void ComputeKinematics(int _offset = 0)
        {
            int _scanIncrement;
            if (ScanAngles == 0)
            {
                _scanIncrement = 180;
            }
            else
            {
                _scanIncrement = 180 / ScanAngles;
            }

            List<int> _angleList = new List<int>();
            List<double> _angleListR = new List<double>();
            int _degAngle = 0;
            ScanVelocities.Clear();
            ScanAccelerations.Clear();

            for (int i = 0; i < ScanAngles; i++)
            {
                decimal[] _accels = new decimal[2];
                decimal[] _velocities = new decimal[2];
                _degAngle = _scanIncrement * i + _offset;
                _angleList.Add(_degAngle);
                _angleListR.Add(_degAngle * Deg2Rad);
                _velocities[0] = (decimal)Math.Round(Math.Abs((decimal)Math.Cos(_angleListR[i]) * ScanVelocity),3);
                _velocities[1] = (decimal)Math.Round(Math.Abs((decimal)Math.Sin(_angleListR[i]) * ScanVelocity),3);
                _accels[0] = (decimal)Math.Round(Math.Abs((decimal)Math.Cos(_angleListR[i]) * ScanAcceleration),3);
                _accels[1] = (decimal)Math.Round(Math.Abs((decimal)Math.Sin(_angleListR[i]) * ScanAcceleration),3);
                if(_velocities[0] == 0.0m)
                {
                    _velocities[0] = ScanVelocity;
                }
                if (_velocities[1] == 0.0m)
                {
                    _velocities[1] = ScanVelocity;
                }
                if(_accels[0] == 0.0m)
                {
                    _accels[0] = ScanAcceleration;
                }
                if (_accels[1] == 0.0m)
                {
                    _accels[1] = ScanAcceleration;
                }
                ScanVelocities.Add(_velocities);
                ScanAccelerations.Add(_accels);
            }
#if RotatedScanWriteOut
            string _outstr = "";
            string _filename = "kinematics-" + RowsRequired.ToString() + "rows.csv";
            for (int i = 0; i < _angleList.Count; i++)
            {
                _outstr += ScanVelocities[i][0].ToString() + "," + ScanVelocities[i][1].ToString()
                    + "," + ScanAccelerations[i][0].ToString() + "," + ScanAccelerations[i][1].ToString()
                    + "\n";
            }
            File.WriteAllText(_filename, _outstr);
            Console.WriteLine("Wrote acceleration and velocity data to {0}", _filename);
#endif

        }
        
        public void ComputeBoundingBoxScans()
        {
            // Add in the zero scan data
            BoundingPointsRequired = new List<int>();
            BoundingRowsRequired = new List<int>();
            BoundingPointsRequired.Add(PointsPerLine);
            BoundingRowsRequired.Add(RowsRequired);
            BoundingBoxScans.Add(ScanCoordinates);

            for (int i = 1; i<RotatedCoordinates.Count; i++)
            {
                decimal xMax = 0m;
                decimal yMax = 0m;
                decimal xMin = 0m;
                decimal yMin = 0m;
                for (int k = 0; k<RotatedCoordinates[i].Count; k++)
                {

                    if (k == 0)
                    {
                        xMax = RotatedCoordinates[i][k][0];
                        yMax = RotatedCoordinates[i][k][1];
                        xMin = xMax;
                        yMin = yMax;
                        if (RotatedCoordinates[i][k][2] < xMin)
                        {
                            xMin = RotatedCoordinates[i][k][2];
                        }
                        else if (RotatedCoordinates[i][k][2] > xMax)
                        {
                            xMax = RotatedCoordinates[i][k][2];
                        }
                        if (RotatedCoordinates[i][k][3] < yMin)
                        {
                            yMin = RotatedCoordinates[i][k][3];
                        }
                        else if (RotatedCoordinates[i][k][3] > yMax)
                        {
                            yMax = RotatedCoordinates[i][k][3];
                        }
                    }
                    else
                    {
                        if(RotatedCoordinates[i][k][0] < xMin)
                        {
                            xMin = RotatedCoordinates[i][k][0];
                        }
                        if (RotatedCoordinates[i][k][0] > xMax)
                        {
                            xMax = RotatedCoordinates[i][k][0];
                        }
                        if (RotatedCoordinates[i][k][2] < xMin)
                        {
                            xMin = RotatedCoordinates[i][k][2];
                        }
                        if (RotatedCoordinates[i][k][2] > xMax)
                        {
                            xMax = RotatedCoordinates[i][k][2];
                        }

                        if (RotatedCoordinates[i][k][1] < yMin)
                        {
                            yMin = RotatedCoordinates[i][k][1];
                        }
                        if (RotatedCoordinates[i][k][1] > yMax)
                        {
                            yMax = RotatedCoordinates[i][k][1];
                        }
                        if (RotatedCoordinates[i][k][3] < yMin)
                        {
                            yMin = RotatedCoordinates[i][k][3];
                        }
                        if (RotatedCoordinates[i][k][3] > yMax)
                        {
                            yMax = RotatedCoordinates[i][k][3];
                        }
                    }
                }
                // Add 1mm of padding to the box to account for misalignment.
                xMin = xMin - 0.25m;
                xMax = xMax + 0.25m;
                yMin = yMin - 0.25m;
                yMax = yMax + 0.25m;
                decimal xD = xMax - xMin;
                decimal yD = yMax - yMin;
                int newPoints = (int)((xD / ScanVelocity) * LaserFrequency);
                int newRows = (int)Math.Ceiling(yD / RowSpacing);
                Console.WriteLine("Scan {0}: Bounds: LL: {1}, {2}\t UR: {3}, {4}\tDELTA: {5}, {6}   Points: {7}, Rows: {8}", i, xMin, yMin,
                    xMax, yMax, xD, yD, newPoints, newRows);
                BoundingPointsRequired.Add(newPoints);
                BoundingRowsRequired.Add(newRows);

                List<decimal[]> tempScan = new List<decimal[]>();
                for(int k = 0; k<newRows; k++)
                {
                    decimal[] tempCoords = new decimal[4];
                    tempCoords[0] = xMin;
                    tempCoords[2] = xMax;
                    tempCoords[1] = yMin + (RowSpacing * k);
                    tempCoords[3] = tempCoords[1];
                    tempScan.Add(tempCoords);
                }
                BoundingBoxScans.Add(tempScan);
                
            }
        }
        #endregion Calculation Helpers

        /// <summary>
        /// Triggers the event handler for IPropertyNotify behavior
        /// </summary>
        /// <param name="name">The name of the calling property as a string.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
