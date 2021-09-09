using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace scanengine
{
    public class ScanVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        #region MLS Properties
        private string _MLSSerialNumber = "73000001";
        public string MLSSerialNumber
        {
            get
            {
                return _MLSSerialNumber;
            }
            set
            {
                _MLSSerialNumber = value;
                MotionControl.Serial = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region MLS ICommands
        private ICommand _StageConnect;
        public ICommand StageConnectCommand
        {
            get
            {
                if(_StageConnect == null)
                {
                    _StageConnect = new RelayCommand(
                        param => StageConnectToggle());
                }
                return _StageConnect;
            }
        }
        private string _StageConnectString = "Connect MLS";
        public string StageConnectString
        {
            get
            {
                return _StageConnectString;
            }
            set
            {
                _StageConnectString = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public ScanController ScanControl = new ScanController();
        public Motion MotionControl = new Motion();
        public void StageConnectToggle()
        {
            if (StageConnectString.Equals("Connect MLS"))
            {
                // Logic for connecting and disconnecting from the MLS203 stage.
                MotionControl.Serial = MLSSerialNumber;
                StageConnectString = "Connecting";
                MotionControl.Connect();
                StageConnectString = "Disconnect";
                Console.WriteLine("Connected. StageConnectString: {0}", StageConnectString);
            }
            else if (StageConnectString.Equals("Disconnect"))
            {
                MotionControl.Disconnect();
                StageConnectString = "Connect MLS";
                Console.WriteLine("Disconnecting");
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    }
}
