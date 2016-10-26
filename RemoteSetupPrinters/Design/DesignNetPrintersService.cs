using System;
using System.Linq;
using RemoteSetupPrinters.Model;
using System.Collections.ObjectModel;

namespace RemoteSetupPrinters.Design
{
    class DesignNetPrintersService : INetPrintersService
    {
        #region Поля
        private ObservableCollection<NetPrinters> _printers;
        #endregion

        #region Конструктор класса
        public DesignNetPrintersService()
        {
            _printers = new ObservableCollection<NetPrinters>();
            _printers.Add(new NetPrinters { IsSelected = false, Name = "Design HP printers", HostName = "HP_IT_Test", IPAdress = "255.255.255.255", TypeDevice = "HPNetPrinter" });
        }
        #endregion

        #region Методы
        public void GetData(Action<ObservableCollection<NetPrinters>, Exception> callback)
        {
            callback(_printers, null);
        }
        public void addPrinter(NetPrinters printer)
        {
            _printers.Add(printer);
        }
        public void ClearAllData()
        {
            _printers.Clear();
        }
        public void SelectAllData()
        {
            _printers.ToList().ForEach(x => x.IsSelected = true);
        }
        public void UnselectAllData()
        {
            _printers.ToList().ForEach(x => x.IsSelected = false);
        }
        #endregion
    }
}
