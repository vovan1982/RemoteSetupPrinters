using System;
using RemoteSetupPrinters.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;

namespace RemoteSetupPrinters.Design
{
    class DesignInstalledPrintersService : IInstalledPrintersService
    {
        #region Поля
        private ObservableCollection<InstalledPrinters> _printers;
        #endregion

        #region Конструктор класса
        public DesignInstalledPrintersService()
        {
            _printers = new ObservableCollection<InstalledPrinters>();
            _printers.Add(new InstalledPrinters { Name = "HPDesign", Port = "HPPortConnect", IPAdress = "10.2.8.12"});
        }
        #endregion

        #region Методы
        public void GetData(Action<ObservableCollection<InstalledPrinters>, Exception> callback)
        {
            callback(_printers, null);
        }
        public void addPrinter(InstalledPrinters printer)
        {
            _printers.Add(printer);
        }
        public void ClearAllData()
        {
            _printers.Clear();
        }
        public void setDefaultPrinter(string printerName)
        {
            _printers.ToList().ForEach(x => { if (x.Name == printerName) x.IsDefault = new BitmapImage(new Uri(@"/RemoteSetupPrinters;component/Resources/defaultPrinter.ico", UriKind.RelativeOrAbsolute)); else x.IsDefault = null; });
        }
        public void updateData(ObservableCollection<InstalledPrinters> newData)
        {
            _printers.Clear();
            newData.ToList().ForEach(x =>
            {
                _printers.Add(x);
            });
        }
        public int SelectedPrinterCount()
        {
            int count = 0;
            _printers.ToList().ForEach(x =>
            {
                if (x.IsSelected) count++;
            });
            return count;
        }
        #endregion
    }
}
