using GalaSoft.MvvmLight;
using System;

namespace RemoteSetupPrinters.Model
{
    public class NetPrinters : ViewModelBase
    {
        private bool _isSelected;
        private string _name;
        private string _hostName;
        private string _iPAdress;
        private string _typeDevice;

        //public delegate void MethodContainer(object sender, EventArgs e);
        //public event MethodContainer onSelectedChanged;
        public NetPrinters()
        {
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                //if (onSelectedChanged != null)
                //{
                //    onSelectedChanged(this,null);
                //}
                RaisePropertyChanged("IsSelected");
            }
        }

        public string IPAdress
        {
            get { return _iPAdress; }
            set { _iPAdress = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string HostName
        {
            get { return _hostName; }
            set { _hostName = value; }
        }

        public string TypeDevice
        {
            get { return _typeDevice; }
            set { _typeDevice = value; }
        }
    }
}