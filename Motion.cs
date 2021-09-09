#define IsSimulation
#define IsReal
#undef IsReal
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.DeviceManagerCLI;


namespace scanengine
{

    public class Motion
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _serial = "";
        private decimal _xpos = 0.00m;
        private decimal _ypos = 0.00m;
        private decimal _xvel = 0.00m;
        private decimal _yvel = 0.00m;
        private decimal _xaccel = 0.00m;
        private decimal _yaccel = 0.00m;


        public string Serial
        {
            get
            {
                return _serial;
            }
            set
            {
                _serial = value;
                NotifyPropertyChanged();
            }
        }
        public decimal XPos
        {
            get
            {
                return _xpos;
            }
            set
            {
                _xpos = value;
                NotifyPropertyChanged();
            }
        }
        public decimal YPos
        {
            get
            {
                return _ypos;
            }
            set
            {
                _ypos = value;
                NotifyPropertyChanged();
            }
        }
        public decimal XVelocity
        {
            get
            {
                return _xvel;
            }
            set
            {
                _xvel = value;
                NotifyPropertyChanged();
            }
        }
        public decimal YVelocity
        {
            get
            { return _yvel; }
            set
            {
                _yvel = value;
                NotifyPropertyChanged();
            }
        }
        public decimal XAcceleration
        {
            get
            {
                return _xaccel;
            }
            set
            {
                _xaccel = value;
                NotifyPropertyChanged();
            }
        }
        public decimal YAcceleration
        {
            get
            {
                return _yaccel;
            }
            set
            {
                _yaccel = value;
                NotifyPropertyChanged();
            }
        }

        public BenchtopBrushlessMotor MLSController;
        public BrushlessMotorChannel XAxis;
        public BrushlessMotorChannel YAxis;

        public BenchtopBrushlessMotorConfiguration XAxisConfig;
        public BenchtopBrushlessMotorConfiguration YAxisConfig;
                                            // Timeouts in ms
        public readonly int InitTimeout = 5000;      // Initializiation 
        public readonly int HomeTimeout = 15000;     // Homing
        public readonly int MoveTimeout = 15000;     // Movement

        private bool _IsConnected = false;
        public bool IsConnected
        {
            get
            {
                return _IsConnected;

            }
            set
            {
                _IsConnected = value;
                NotifyPropertyChanged();
            }
        }
        public Motion()
        {
        }

        public void Connect()
        {
#if IsSimulation
            SimulationManager.Instance.InitializeSimulations();
#endif
#if IsReal
#endif
            DeviceManagerCLI.BuildDeviceList();
            MLSController = BenchtopBrushlessMotor.CreateBenchtopBrushlessMotor(Serial);
            MLSController.Connect(Serial);
            
            XAxis = MLSController.GetChannel(1);
            XAxis.EnableDevice();
            XAxis.WaitForSettingsInitialized(InitTimeout);
            
            YAxis = MLSController.GetChannel(2);
            YAxis.WaitForSettingsInitialized(InitTimeout);
            YAxis.EnableDevice();
            // Subscribe to connection event handler
            IsConnected = true;
            XAxis.StartPolling(20);
            YAxis.StartPolling(20);
        }
        public void Disconnect()
        {
            MLSController.Disconnect(true);
#if IsSimulation
            SimulationManager.Instance.UninitializeSimulations();
#endif
            IsConnected = false;
        }

        
        private void MLSController_ConnectionStateChanged(object sender, ThorlabsConnectionManager.ConnectionStateChangedEventArgs e)
        {
            Console.WriteLine("Caught {0} event.", e.ConnectionState.ToString());
            if(e.ConnectionState.ToString().Equals("Disconnected"))
            {
                IsConnected = false;
            } else if(e.ConnectionState.ToString().Equals("Connected"))
            {
                IsConnected = true;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
