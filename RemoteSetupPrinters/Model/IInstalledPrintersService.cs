using System;
using System.Collections.ObjectModel;

namespace RemoteSetupPrinters.Model
{
    public interface IInstalledPrintersService
    {
        void GetData(Action<ObservableCollection<InstalledPrinters>, Exception> callback);
        void addPrinter(InstalledPrinters printer);
        void ClearAllData();
        void setDefaultPrinter(string printerName);
        void updateData(ObservableCollection<InstalledPrinters> newData);
        int SelectedPrinterCount();
    }
}
