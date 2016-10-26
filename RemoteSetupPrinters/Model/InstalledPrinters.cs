using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
namespace RemoteSetupPrinters.Model
{
    public class InstalledPrinters : ViewModelBase
    {
        private string _name;
        private string _port;
        private string _iPAdress;
        private string _regKey;
        private ImageSource _isDefault;
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

        public InstalledPrinters()
        {
            _isDefault = null;
        }

        public string IPAdress
        {
            get { return _iPAdress; }
            set { _iPAdress = value; }
        }

        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string RegKey
        {
            get { return _regKey; }
            set { _regKey = value; }
        }

        public ImageSource IsDefault
        {
            get { return _isDefault; }
            set 
            { 
                _isDefault = value;
                RaisePropertyChanged("IsDefault");
            }
        }
    }
}
