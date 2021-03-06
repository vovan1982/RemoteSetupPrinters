﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RemoteSetupPrinters.Model
{
    class NetPrintersService : INetPrintersService
    {
        #region Поля
        private ObservableCollection<NetPrinters> _printers;
        #endregion

        #region Конструктор класса
        public NetPrintersService()
        {
            _printers = new ObservableCollection<NetPrinters>();
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
