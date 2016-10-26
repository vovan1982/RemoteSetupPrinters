using System;
using System.Collections.ObjectModel;

namespace RemoteSetupPrinters.Model
{
    public interface INetPrintersService
    {
        void GetData(Action<ObservableCollection<NetPrinters>, Exception> callback);
        void addPrinter(NetPrinters printer);
        void ClearAllData();
        void SelectAllData();
        void UnselectAllData();
    }
}
