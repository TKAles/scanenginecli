using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace scanengine
{
    public class ScanController
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _MLSSerialNumber;
        public string MLSSerialNumber
        {
            get
            {
                return _MLSSerialNumber;
            }
            set
            {
                _MLSSerialNumber = value;
                NotifyPropertyChanged();
            }
        }
        public ScanController()
        {

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
