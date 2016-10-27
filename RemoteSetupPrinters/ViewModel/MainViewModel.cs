using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;
using RemoteSetupPrinters.Model;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using SnmpSharpNet;
using System.Collections.Generic;
using System.Windows.Documents;
using System.IO;
using RemoteSetupPrinters.ProgressDialog;
using System.Management;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace RemoteSetupPrinters.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Поля
        private ICommand _on_btScan_click;
        private ICommand _on_selectAll_click;
        private ICommand _on_btSetup_click;
        private ICommand _on_netPrintersItem_click;
        private ICommand _on_netPrinterItemClick;
        private ICommand _on_btDelPrintersOnRemPCClick;
        private ICommand _on_btSetDefaultPrintersOnRemPCClick;
        private ICommand _on_btPrintTestPagePrintersOnRemPCClick;
        private ICommand _on_UserOnRemPCChanged;
        private ICommand _menuCopyCurrent;
        private ICommand _menuCopySelected;
        private ICommand _menuCopyAll;
        private ICommand _modeChanged;
        private ICommand _on_NameHostsKeyDown;
        private string _nameHosts;
        private string _prevNameHosts;
        private string _network;
        private string _curUserOnRemPC;
        private string _filterText;
        private string _statusText;
        private string _btScan_content;
        private string _selectedUserOnRemPC;
        private int _firstItem;
        private int _lastItem;
        private int _installedPrintersOnRemPCIndex;
        private bool _getNetFromNameRemPC_isChecked;
        private bool _isEnableNameHost;
        private bool _isEnableFirstItem;
        private bool _isEnableLastItem;
        private bool _isEnablebtScan;
        private bool _isEnableOneCompMode;
        private bool _isEnableListCompMode;
        private bool _isEnableGetNetFromNameRemPC;
        private bool _isEnableListNetPrinters;
        private bool _isEnableListPrintersOnRemPC;
        private bool _isEnableCurUserListOnRemPC;
        private bool _networkIsEnable;
        private bool? _allSelected;
        private bool _scanIsRuned;
        private bool _oneCompMode;
        private bool _listCompMode;
        private bool _printersOnRemPCButtonIsEnable;
        private Visibility _isVisiblePrintersOnRemPCGroup;
        private ObservableCollection<NetPrinters> _snmpNetPrinters;
        private ObservableCollection<InstalledPrinters> _installedPrintersOnRemPC;
        private InstalledPrinters _installedPrintersOnRemPCItem;
        private NetPrinters _selectedNetPrinterItem;
        private CollectionViewSource _installedPrintersOnRemPCView;
        private IInstalledPrintersService _installedPrinters;
        private INetPrintersService _netPrinters;
        private Dictionary<string, string> _typesNetPrinters;
        private Dictionary<string, ObservableCollection<InstalledPrinters>> _installedPrintersOnRemPCWithUsers;
        private List<string> _userOnRemPC;
        private SNMPConfig _snmpConfig;
        #endregion

        #region Свойства
        public ICommand On_btScan_click
        {
            get
            {
                return _on_btScan_click ?? (_on_btScan_click = new RelayCommand(() =>
                {
                    if (!_scanIsRuned)
                    {
                        _scanIsRuned = true;
                        if (OneCompMode)
                            StartCheckingAndGetData();
                        else if (ListCompMode)
                            StartCheckingAndGetDataListCompMode();
                    }
                    else
                    {
                        _scanIsRuned = false;
                        IsEnableBtScan = false;
                    }
                }));
            }
        }
        public ICommand On_selectAll_click
        {
            get 
            {
                return _on_selectAll_click ?? (_on_selectAll_click = new RelayCommand(() => 
                {
                    // Если список сетевых принтеров пуст AllSelected = false
                    if (SnmpNetPrinters.Count > 0)
                    {
                        if (AllSelected != null)
                        {
                            if (AllSelected == true)
                                _netPrinters.SelectAllData();
                            else
                                _netPrinters.UnselectAllData();
                        }
                        else
                        {
                            _netPrinters.UnselectAllData();
                            AllSelected = AllItemSelected(_snmpNetPrinters);
                        }
                    }
                    else
                    {
                        AllSelected = false;
                    }
                })); 
            }
        }
        public ICommand On_btSetup_click
        {
            get 
            {
                return _on_btSetup_click ?? (_on_btSetup_click = new RelayCommand(() =>
                    {
                        if (OneCompMode)
                            SetUpSelectedPrinters();
                        else if (ListCompMode)
                            SetUpSelectedPrintersListCompMode();
                    }));
            }
        }
        public ICommand On_netPrintersItem_click
        {
            get 
            {
                return _on_netPrintersItem_click ?? (_on_netPrintersItem_click = new RelayCommand(() => 
                {
                    UpdateAllSelectedCheckBox();
                })); 
            }
        }
        public ICommand On_netPrinterItemClick
        {
            get 
            {
                return _on_netPrinterItemClick ?? (_on_netPrinterItemClick = new RelayCommand(() => 
                {
                    if (SelectedNetPrinterItem != null)
                    {
                        if (SelectedNetPrinterItem.IsSelected)
                        {
                            SelectedNetPrinterItem.IsSelected = false;
                        }
                        else
                        {
                            SelectedNetPrinterItem.IsSelected = true;
                        }
                    }
                })); 
            }
        }
        public ICommand On_btDelPrintersOnRemPCClick
        {
            get
            {
                return _on_btDelPrintersOnRemPCClick ?? (_on_btDelPrintersOnRemPCClick = new RelayCommand(() =>
                {
                    if (InstalledPrintersOnRemPCItem != null)
                    {
                        MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить выбранный принтер?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            DelPrintersOnRemPC();
                        }
                    }
                }));
            }
        }
        public ICommand On_btSetDefaultPrintersOnRemPCClick
        {
            get 
            {
                return _on_btSetDefaultPrintersOnRemPCClick ?? (_on_btSetDefaultPrintersOnRemPCClick = new RelayCommand(() =>
                {
                    if (InstalledPrintersOnRemPCItem != null)
                    {
                        SetDefaultPrintersOnRemPC();
                    }
                }));
            }
            
        }
        public ICommand On_btPrintTestPagePrintersOnRemPCClick
        {
            get 
            {
                return _on_btPrintTestPagePrintersOnRemPCClick ?? (_on_btPrintTestPagePrintersOnRemPCClick = new RelayCommand(() =>
                {
                    if (InstalledPrintersOnRemPCItem != null)
                    {
                        PrintTestPagePrintersOnRemPC();
                    }
                }));
            }
        }
        public ICommand MenuCopyAll
        {
            get 
            { 
                return _menuCopyAll ?? (_menuCopyAll = new RelayCommand(() =>
                {
                    string data = "";
                    SnmpNetPrinters.ToList().ForEach(x =>
                    {
                        data += x.Name + "\t" + x.HostName + "\t" + x.IPAdress + "\t" + x.TypeDevice + "\n";
                    });
                    Clipboard.SetDataObject(data.Remove(data.Length - 1));
                })); 
            }
        }
        public ICommand MenuCopySelected
        {
            get 
            { 
                return _menuCopySelected ?? (_menuCopySelected = new RelayCommand(() =>
                {
                    string data = "";
                    SnmpNetPrinters.ToList().ForEach(x => 
                    {
                        if (x.IsSelected == true)
                        {
                            data += x.Name + "\t" + x.HostName + "\t" + x.IPAdress + "\t" + x.TypeDevice + "\n";
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(data))
                        Clipboard.SetDataObject(data.Remove(data.Length - 1));
                    else
                        MessageBox.Show("Нет выбранных элементов!!!");
                }));
            }
        }
        public ICommand MenuCopyCurrent
        {
            get 
            { 
                return _menuCopyCurrent ?? (_menuCopyCurrent = new RelayCommand(() =>
                {
                    if (SelectedNetPrinterItem != null)
                        Clipboard.SetDataObject(SelectedNetPrinterItem.Name + "\t" + SelectedNetPrinterItem.HostName + "\t" + SelectedNetPrinterItem.IPAdress + "\t" + SelectedNetPrinterItem.TypeDevice);
                    else
                        MessageBox.Show("Нет выделенных элементов!!!");
                }));
            }
        }
        public ICommand ModeChanged
        {
            get 
            {
                return _modeChanged ?? (_modeChanged = new RelayCommand(() =>
                    {
                        if (OneCompMode)
                            SetOneCompMode();
                        else if (ListCompMode)
                            SetListCompMode();
                        else
                            MessageBox.Show("Режим не определен!!");
                    }));
            }
        }
        public ICommand On_UserOnRemPCChanged
        {
            get 
            {
                return _on_UserOnRemPCChanged ?? (_on_UserOnRemPCChanged = new RelayCommand(() =>
                {
                    ChangeUserOnRemPC();
                }));
            }
        }
        public ICommand On_NameHostsKeyDown
        {
            get 
            {
                return _on_NameHostsKeyDown ?? (_on_NameHostsKeyDown = new RelayCommand(() =>
                {
                    if (!_scanIsRuned)
                    {
                        _scanIsRuned = true;
                        if (OneCompMode)
                            StartCheckingAndGetData();
                        else if (ListCompMode)
                            StartCheckingAndGetDataListCompMode();
                    }
                }));
            }
        }
        public bool? AllSelected
        {
            get { return _allSelected; }
            set 
            {
                _allSelected = value;
                RaisePropertyChanged("AllSelected");
                RaisePropertyChanged("IsBtSetupEnable");
            }
        }
        public bool IsEnableNameHost
        {
            get { return _isEnableNameHost; }
            set 
            { 
                if (_isEnableNameHost != value)
                {
                    _isEnableNameHost = value;
                    RaisePropertyChanged("IsEnableNameHost");
                }
            }
        }
        public bool IsEnableFirstItem
        {
            get { return _isEnableFirstItem; }
            set 
            { 
                if (_isEnableFirstItem != value)
                {
                    _isEnableFirstItem = value;
                    RaisePropertyChanged("IsEnableFirstItem");
                }
            }
        }
        public bool IsEnableLastItem
        {
            get { return _isEnableLastItem; }
            set 
            { 
                if (_isEnableLastItem != value)
                {
                    _isEnableLastItem = value;
                    RaisePropertyChanged("IsEnableLastItem");
                }
            }
        }
        public bool IsEnableBtScan
        {
            get { return _isEnablebtScan; }
            set 
            { 
                if (_isEnablebtScan != value)
                {
                    _isEnablebtScan = value;
                    RaisePropertyChanged("IsEnablebtScan");
                }
            }
        }
        public bool IsEnableOneCompMode
        {
            get { return _isEnableOneCompMode; }
            set
            {
                if (_isEnableOneCompMode != value)
                {
                    _isEnableOneCompMode = value;
                    RaisePropertyChanged("IsEnableOneCompMode");
                }
            }
        }
        public bool IsEnableListCompMode
        {
            get { return _isEnableListCompMode; }
            set
            {
                if (_isEnableListCompMode != value)
                {
                    _isEnableListCompMode = value;
                    RaisePropertyChanged("IsEnableListCompMode");
                }
            }
        }
        public bool IsEnableGetNetFromNameRemPC
        {
            get { return _isEnableGetNetFromNameRemPC; }
            set 
            { 
                if (_isEnableGetNetFromNameRemPC != value)
                {
                    _isEnableGetNetFromNameRemPC = value;
                    RaisePropertyChanged("IsEnableGetNetFromNameRemPC");
                }
            }
        }
        public bool IsBtSetupEnable
        {
            get 
            {
                if (_allSelected == null || _allSelected == true){
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsEnableListNetPrinters
        {
            get { return _isEnableListNetPrinters; }
            set 
            {
                if (_isEnableListNetPrinters != value)
                {
                    _isEnableListNetPrinters = value;
                    RaisePropertyChanged("IsEnableListNetPrinters");
                }
            }
        }
        public bool GetNetFromNameRemPC_isChecked
        {
            get 
            { 
                return _getNetFromNameRemPC_isChecked; 
            }
            set 
            { 
                if (_getNetFromNameRemPC_isChecked != value)
                {
                    _getNetFromNameRemPC_isChecked = value;
                    NetworkIsEnable = !value;
                    RaisePropertyChanged("GetNetFromNameRemPC_isChecked");
                    RaisePropertyChanged("NetworkIsEnable");
                }
                RaisePropertyChanged("NetworkIsEnable");
            }
        }
        public bool NetworkIsEnable
        {
            get
            {
                return _networkIsEnable;
            }
            set
            {
                if (_networkIsEnable != value)
                {
                    _networkIsEnable = value;
                    RaisePropertyChanged("NetworkIsEnable");
                }
            }
        }
        public bool IsEnableListNetPrintersMenu
        {
            get 
            { 
                return SnmpNetPrinters.Count > 0;
            }
        }
        public bool IsEnableListPrintersOnRemPC
        {
            get 
            { 
                return _isEnableListPrintersOnRemPC; 
            }
            set 
            { 
                if (_isEnableListPrintersOnRemPC != value)
                {
                    _isEnableListPrintersOnRemPC = value;
                    RaisePropertyChanged("IsEnableListPrintersOnRemPC");
                }
            }
        }
        public bool IsEnableCurUserListOnRemPC
        {
            get { return _isEnableCurUserListOnRemPC; }
            set 
            { 
                if (_isEnableCurUserListOnRemPC != value)
                {
                    _isEnableCurUserListOnRemPC = value;
                    RaisePropertyChanged("IsEnableCurUserListOnRemPC");
                }
            }
        }
        public bool ListCompMode
        {
            get { return _listCompMode; }
            set 
            { 
                if (_listCompMode != value)
                {
                    _listCompMode = value;
                    RaisePropertyChanged("ListCompMode");
                }
            }
        }
        public bool OneCompMode
        {
            get { return _oneCompMode; }
            set 
            { 
                if (_oneCompMode != value)
                {
                    _oneCompMode = value;
                    RaisePropertyChanged("OneCompMode");
                }
            }
        }
        public bool PrintersOnRemPCButtonIsEnable
        {
            get
            {
                return _printersOnRemPCButtonIsEnable;
            }
            set
            {
                if (_printersOnRemPCButtonIsEnable != value)
                {
                    _printersOnRemPCButtonIsEnable = value;
                    RaisePropertyChanged("PrintersOnRemPCButtonIsEnable");
                }
            }
        }
        public string NameHosts
        {
            get { return _nameHosts; }
            set 
            { 
                if (_nameHosts != value)
                {
                    _nameHosts = value;
                    RaisePropertyChanged("NameHosts");
                }
            }
        }
        public string PrevNameHosts
        {
            get { return _prevNameHosts; }
            private set { _prevNameHosts = value; }
        }
        public string Network
        {
            get { return _network; }
            set 
            { 
                if (_network != value)
                {
                    _network = value;
                    RaisePropertyChanged("Network");
                }
            }
        }
        public string CurUserOnRemPC
        {
            get { return _curUserOnRemPC; }
            set
            {
                if (_curUserOnRemPC != value)
                {
                    _curUserOnRemPC = value;
                    RaisePropertyChanged("CurUserOnRemPC");
                }
            }
        }
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                _installedPrintersOnRemPCView.View.Refresh();
                RaisePropertyChanged("FilterText");
            }
        }
        public string BtScan_content
        {
            get { return _btScan_content; }
            set
            {
                if (_btScan_content != value)
                {
                    _btScan_content = value;
                    RaisePropertyChanged("BtScan_content");
                }
            }
        }
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    RaisePropertyChanged("StatusText");
                }
            }
        }
        public string SelectedUserOnRemPC
        {
            get { return _selectedUserOnRemPC; }
            set 
            { 
                if (_selectedUserOnRemPC != value)
                {
                    _selectedUserOnRemPC = value;
                    RaisePropertyChanged("SelectedUserOnRemPC");
                }
            }
        }
        public int FirstItem
        {
            get { return _firstItem; }
            set
            {
                if (value <= _lastItem)
                {
                    if (_firstItem != value)
                    {
                        _firstItem = value;
                        RaisePropertyChanged("FirstItem");
                    }
                }
                else
                {
                    MessageBox.Show("Начальное значение не может быть больше конечного!!");
                }
            }
        }
        public int LastItem
        {
            get { return _lastItem; }
            set
            {
                if (value >= _firstItem)
                {
                    if (_lastItem != value)
                    {
                        _lastItem = value;
                        RaisePropertyChanged("LastItem");
                    }
                }
                else
                {
                    MessageBox.Show("Конечное значение не может быть меньше начального!!");
                }
            }
        }
        public int InstalledPrintersOnRemPCIndex
        {
            get
            {
                return _installedPrintersOnRemPCIndex;
            }
            set
            {
                if (_installedPrintersOnRemPCIndex != value)
                {
                    _installedPrintersOnRemPCIndex = value;
                    RaisePropertyChanged("InstalledPrintersOnRemPCIndex");
                    PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty();
                }
            }
        }
        public Visibility IsVisiblePrintersOnRemPCGroup
        {
            get { return _isVisiblePrintersOnRemPCGroup; }
            set
            {
                if (_isVisiblePrintersOnRemPCGroup != value)
                {
                    _isVisiblePrintersOnRemPCGroup = value;
                    RaisePropertyChanged("IsVisiblePrintersOnRemPCGroup");
                }
            }
        }
        public ObservableCollection<NetPrinters> SnmpNetPrinters
        {
            get { return _snmpNetPrinters; }
            set 
            { 
                _snmpNetPrinters = value;
                RaisePropertyChanged("SnmpNetPrinters");
                RaisePropertyChanged("IsEnableListNetPrintersMenu");
            }
        }
        public ObservableCollection<InstalledPrinters> InstalledPrintersOnRemPC
        {
            get { return _installedPrintersOnRemPC; }
            set 
            {
                if (value != null)
                {
                    _installedPrintersOnRemPC = value;
                    _installedPrintersOnRemPCView = new CollectionViewSource();
                    _installedPrintersOnRemPCView.Source = _installedPrintersOnRemPC;
                    _installedPrintersOnRemPCView.Filter += installedPrintersOnRemPC_Filter;
                    _installedPrintersOnRemPCView.View.CurrentChanged += PrintersOnRemPCSelectionChanged;
                }
                else
                {
                    _installedPrintersOnRemPC = value;
                    _installedPrintersOnRemPCView = new CollectionViewSource();
                }
                
                RaisePropertyChanged("InstalledPrintersOnRemPC");
            }
        }
        public ListCollectionView InstalledPrintersOnRemPCView
        {
            get
            {
                return (ListCollectionView)_installedPrintersOnRemPCView.View;
            }
        }
        public InstalledPrinters InstalledPrintersOnRemPCItem
        {
            get
            {
                return _installedPrintersOnRemPCItem;
            }
            set
            {
                if (_installedPrintersOnRemPCItem != value)
                {
                    _installedPrintersOnRemPCItem = value;
                    RaisePropertyChanged("InstalledPrintersOnRemPCItem");
                }
            }
        }
        public NetPrinters SelectedNetPrinterItem
        {
            get { return _selectedNetPrinterItem; }
            set 
            { 
                if (_selectedNetPrinterItem != value)
                {
                    _selectedNetPrinterItem = value;
                    RaisePropertyChanged("SelectedNetPrinterItem");
                }
            }
        }
        public List<string> UserOnRemPC
        {
            get { return _userOnRemPC; }
            set 
            { 
                if (_userOnRemPC != value)
                {
                    _userOnRemPC = value;
                    RaisePropertyChanged("UserOnRemPC");
                }
            }
        }
        #endregion

        #region Конструктор класса
        public MainViewModel(IInstalledPrintersService installedPrinters, INetPrintersService netPrinters)
        {
            _snmpConfig = new SNMPConfig();
            Dictionary<string, string> data = null;
            string errorMessage = "";
            try
            {
                data = GetSNMPConfig(Environment.CurrentDirectory + @"\SNMPCONFIG.cfg");
            }
            catch (Exception e)
            {
                ShowErrorMessage(e.Message + "\r\nБудут использованы значения по умолчанию: SNMPVER=2, READCOMMUNITY=Public, TIMEOUT=2000, RETRIES = 1, PORT=161");
            }
            if (data != null)
            {
                if (!CheckConfig(data, ref errorMessage))
                {
                    ShowErrorMessage(errorMessage + "\r\nБудут использованы значения по умолчанию: SNMPVER=2, READCOMMUNITY=Public, TIMEOUT=2000, RETRIES = 1, PORT=161");
                }
                else
                {
                    int j;
                    _snmpConfig.SNMPVER = data["SNMPVER"];
                    if (!string.IsNullOrWhiteSpace(data["READCOMMUNITY"]))
                        _snmpConfig.READCOMMUNITY = data["READCOMMUNITY"];
                    else
                        _snmpConfig.READCOMMUNITY = "public";
                    Int32.TryParse(data["TIMEOUT"], out j);
                    _snmpConfig.TIMEOUT = j;
                    Int32.TryParse(data["RETRIES"], out j);
                    _snmpConfig.RETRIES = j;
                    Int32.TryParse(data["PORT"], out j);
                    _snmpConfig.PORT = j;
                    if (data["SNMPVER"] == "3")
                    {
                        if (!string.IsNullOrWhiteSpace(data["USER"]))
                            _snmpConfig.USER = data["USER"];
                        else
                            _snmpConfig.USER = "initial";
                        _snmpConfig.AUTHALGORITHM = data["AUTHALGORITHM"].ToUpper();
                        _snmpConfig.PASSAUTH = data["PASSAUTH"];
                        _snmpConfig.PRIVACYALGORITHM = data["PRIVACYALGORITHM"].ToUpper();
                        _snmpConfig.PASSPRIVACY = data["PASSPRIVACY"];
                        if (data.ContainsKey("CONTEXTNAME"))
                            _snmpConfig.CONTEXTNAME = data["CONTEXTNAME"];
                    }
                }
            }
            _btScan_content = "Сканировать";
            _getNetFromNameRemPC_isChecked = true;
            _isEnableNameHost = true;
            _isEnablebtScan = true;
            _isEnableFirstItem = true;
            _isEnableLastItem = true;
            _isEnableOneCompMode = true;
            _isEnableListCompMode = true;
            _isEnableGetNetFromNameRemPC = true;
            _isEnableCurUserListOnRemPC = true;
            _firstItem = 1;
            _lastItem = 100;
            _allSelected = false;
            _isEnableListNetPrinters = true;
            _isEnableListPrintersOnRemPC = true;
            _scanIsRuned = false;
            _listCompMode = false;
            _oneCompMode = true;
            _isVisiblePrintersOnRemPCGroup = Visibility.Visible;
            _typesNetPrinters = GetTypesNetPrinters();
            _installedPrinters = installedPrinters;
            _installedPrinters.GetData(
                (printers, error) => 
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    InstalledPrintersOnRemPC = printers;
                });

            _netPrinters = netPrinters;
            _netPrinters.GetData(
                (snmpprinters, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    SnmpNetPrinters = snmpprinters;
                });
            _allSelected = AllItemSelected(_snmpNetPrinters);
        }
        #endregion

        #region Методы
        private void SetListCompMode()
        {
            GetNetFromNameRemPC_isChecked = false;
            IsEnableGetNetFromNameRemPC = false;
            IsVisiblePrintersOnRemPCGroup = Visibility.Collapsed;
            _installedPrinters.ClearAllData();
            _netPrinters.ClearAllData();
            AllSelected = false;
            UserOnRemPC = new List<string>();
            Network = "";
            CurUserOnRemPC = "";
            NameHosts = "";
        }
        private void SetOneCompMode()
        {
            GetNetFromNameRemPC_isChecked = true;
            IsEnableGetNetFromNameRemPC = true;
            IsVisiblePrintersOnRemPCGroup = Visibility.Visible;
            UserOnRemPC = new List<string>();
            _installedPrinters.ClearAllData();
            _netPrinters.ClearAllData();
            AllSelected = false;
            Network = "";
            CurUserOnRemPC = "";
            NameHosts = "";
        }
        private static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return pingable;
            }
            return pingable;
        }
        private bool PrintersOnRemPCViewNotEmpty()
        {
            return InstalledPrintersOnRemPCView != null && (InstalledPrintersOnRemPCIndex >= 0 && InstalledPrintersOnRemPCView.Count > 0);
        }
        private bool InputDataIsValid()
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(NameHosts))
            {
                if (PingHost(NameHosts))
                {
                    if (GetNetFromNameRemPC_isChecked)
                    {
                        result = true;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(Network))
                        {
                            string[] arrNet = Network.Split('.');
                            arrNet[0] = arrNet[0].Replace("_", "");
                            arrNet[1] = arrNet[1].Replace("_", "");
                            arrNet[2] = arrNet[2].Replace("_", "");
                            if (string.IsNullOrWhiteSpace(arrNet[0]) || string.IsNullOrWhiteSpace(arrNet[1]) || string.IsNullOrWhiteSpace(arrNet[2]))
                            {
                                MessageBox.Show("Подсеть для поиска указанна не корректно!!");
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(arrNet[0].Replace("0", "")))
                                {
                                    MessageBox.Show("Подсеть для поиска указанна не корректно!!");
                                }
                                else
                                {
                                    result = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не указанна подсеть для поиска!!");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Удаленный компьютер не доступен!!");
                }
            }
            else
            {
                MessageBox.Show("Не указан удаленный компьютер!!");
            }
            return result;
        }
        private bool InputDataIsValidListCompMode()
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(Network))
            {
                string[] arrNet = Network.Split('.');
                arrNet[0] = arrNet[0].Replace("_", "");
                arrNet[1] = arrNet[1].Replace("_", "");
                arrNet[2] = arrNet[2].Replace("_", "");
                if (string.IsNullOrWhiteSpace(arrNet[0]) || string.IsNullOrWhiteSpace(arrNet[1]) || string.IsNullOrWhiteSpace(arrNet[2]))
                {
                    MessageBox.Show("Подсеть для поиска указанна не корректно!!");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(arrNet[0].Replace("0", "")))
                    {
                        MessageBox.Show("Подсеть для поиска указанна не корректно!!");
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Не указанна подсеть для поиска!!");
            }
            return result;
        }
        private string GetPrintersInfoFromRemotePC(string remotePC)
        {
            RegistryKey printKeyLM;
            String str = "";
            try
            {
                printKeyLM = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, remotePC);
                String[] namesLM = printKeyLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers").GetSubKeyNames();
                foreach (String p in namesLM)
                {
                    str += p + ";";
                    RegistryKey LocalMachineKey = printKeyLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers\" + p);
                    var port = LocalMachineKey.GetValue("Port");
                    if (port != null)
                    {
                        str += (string)port + ";";
                    }
                    else
                    {
                        str += ";";
                    }
                    RegistryKey DriverKey = printKeyLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers\" + p + "\\PrinterDriverData");
                    if (DriverKey != null)
                    {
                        var ipadress = DriverKey.GetValue("HPEWSIPAddress");
                        if (ipadress != null)
                        {
                            str += ((string)ipadress).Split(',').GetValue(0).ToString() + ";|";
                        }
                        else
                        {
                            str += ";|";
                        }
                    }
                    else
                    {
                        str += ";|";
                    }
                }
            }
            catch (Exception e)
            {
                str = "error: " + e.ToString();
            }
            return str;
        }
        private void UpdateSelectionPrintersOnRemPC()
        {
            if (_installedPrintersOnRemPCView != null && _installedPrintersOnRemPCView.View != null && _installedPrintersOnRemPCView.View.CurrentItem == null)
            {
                if (InstalledPrintersOnRemPC.Count > 0)
                    InstalledPrintersOnRemPCItem = InstalledPrintersOnRemPC.First();
                if (InstalledPrintersOnRemPCView.Count > 0)
                    InstalledPrintersOnRemPCView.MoveCurrentToFirst();
            }
        }
        private void PrintersOnRemPCSelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectionPrintersOnRemPC();
        }
        private void installedPrintersOnRemPC_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                e.Accepted = true;
                return;
            }

            InstalledPrinters prn = e.Item as InstalledPrinters;
            if (prn != null)
            {
                if (prn.Name.ToUpper().Contains(FilterText.ToUpper()))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
        }
        private void StartCheckingAndGetData()
        {
            Dictionary<string, ObservableCollection<InstalledPrinters>> _printers = new Dictionary<string, ObservableCollection<InstalledPrinters>>();
            PrevNameHosts = NameHosts;
            string net = "";
            bool error = false;
            string errorMessage = "";
            // Запускаем новый поток
            Task.Factory.StartNew(() =>
            {
                // Усыпляем ненадолго поток
                Thread.Sleep(10);
                // Вызываем изменение текста лейбла в потоке UI
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Проверка указанных данных");
                DispatcherHelper.CheckBeginInvokeOnUI(() => BtScan_content = "Остановить сканирование");
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = false);
                
                if (InputDataIsValid())
                {
                    if (GetNetFromNameRemPC_isChecked)
                    {
                        IPAddress ip = new IPAddress(0);
                        foreach (IPAddress currrentIPAddress in Dns.GetHostAddresses(NameHosts))
                        {
                            if (currrentIPAddress.AddressFamily.ToString() == System.Net.Sockets.AddressFamily.InterNetwork.ToString())
                            {
                                ip = currrentIPAddress;
                                break;
                            }
                        }

                        string[] arrNet = ip.ToString().Split('.');
                        if (arrNet[0] != "0")
                        {
                            net = arrNet[0] + "." + arrNet[1] + "." + arrNet[2] + ".";
                        }
                        else
                        {
                            errorMessage += "Не удалось получить подсеть из имени удаленного компьютера!!";
                            error = true;
                        }
                    }
                    else
                    {
                        string[] arrNet = Network.Split('.');
                        arrNet[0] = arrNet[0].Replace("_", "");
                        arrNet[1] = arrNet[1].Replace("_", "");
                        arrNet[2] = arrNet[2].Replace("_", "");
                        if (arrNet[0].Substring(0, 1) == "0")
                        {
                            arrNet[0] = arrNet[0].Substring(1);
                            if (arrNet[0].Substring(0, 1) == "0")
                            {
                                arrNet[0] = arrNet[0].Substring(1);
                            }
                        }
                        if (arrNet[1].Substring(0, 1) == "0")
                        {
                            if (!string.IsNullOrWhiteSpace(arrNet[1].Substring(1)))
                            {
                                arrNet[1] = arrNet[1].Substring(1);
                                if (arrNet[1].Substring(0, 1) == "0")
                                {
                                    if (!string.IsNullOrWhiteSpace(arrNet[1].Substring(1)))
                                    {
                                        arrNet[1] = arrNet[1].Substring(1);
                                    }
                                }
                            }
                        }
                        if (arrNet[2].Substring(0, 1) == "0")
                        {
                            if (!string.IsNullOrWhiteSpace(arrNet[2].Substring(1)))
                            {
                                arrNet[2] = arrNet[2].Substring(1);
                                if (arrNet[2].Substring(0, 1) == "0")
                                {
                                    if (!string.IsNullOrWhiteSpace(arrNet[2].Substring(1)))
                                    {
                                        arrNet[2] = arrNet[2].Substring(1);
                                    }
                                }
                            }
                        }
                        net = arrNet[0] + "." + arrNet[1] + "." + arrNet[2] + ".";
                    }
                    // подключаемся к удаленному компу и считываем список принтеров в модель
                    if (!error && _scanIsRuned)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Получения списка установленных принтеров с удаленного компьютера");
                        string errStr = "";
                        _installedPrintersOnRemPCWithUsers = null;
                        DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrintersOnRemPCWithUsers = GetInstalledPrintersOnRemotePC(NameHosts, ref error, ref errStr));
                        while (_installedPrintersOnRemPCWithUsers == null)
                        {
                            Thread.Sleep(500);
                        }
                        if (error)
                        {
                            errorMessage += "Ошибка получения информации об установленных принтерах:\r\n" + errStr;
                        }
                        if (!error && _scanIsRuned)
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() => UserOnRemPC = _installedPrintersOnRemPCWithUsers.Keys.ToList());
                            DispatcherHelper.CheckBeginInvokeOnUI(() => SelectedUserOnRemPC = UserOnRemPC[0]);
                            DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrinters.updateData(_installedPrintersOnRemPCWithUsers[UserOnRemPC[0]]));
                        }
                    }
                    if (!error && _scanIsRuned)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Сканирование сети на наличие сетевых принтеров");
                        DispatcherHelper.CheckBeginInvokeOnUI(() => AllSelected = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => _netPrinters.ClearAllData());
                        for (int i = FirstItem; i <= LastItem; i++)
                        {
                            if (!_scanIsRuned) break;
                            DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Сканирование сети на наличие сетевых принтеров " + net + i);
                            if (PingHost(net + i))
                            {
                                string printInfo = "";
                                if (App.mArgs != null && App.mArgs.Length > 0)
                                {
                                    if (App.mArgs[0].ToUpper() == "-DEBUG")
                                    {
                                        StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\snmperrorlog_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true);
                                        printInfo = GetSMNPPrinterInfo(net + i, _snmpConfig, sw);
                                        sw.Close();
                                    }
                                    else
                                    {
                                        printInfo = GetSMNPPrinterInfo(net + i, _snmpConfig);
                                    }
                                }
                                else
                                {
                                    printInfo = GetSMNPPrinterInfo(net + i, _snmpConfig);
                                }
                                if (!printInfo.Contains("error"))
                                {
                                    string[] arrNetPrinter = printInfo.Split(';');
                                    if (arrNetPrinter.GetUpperBound(0) > 0)
                                    {
                                        DispatcherHelper.CheckBeginInvokeOnUI(() => _netPrinters.addPrinter(new NetPrinters() { IsSelected = false, Name = arrNetPrinter[0], HostName = arrNetPrinter[1], IPAdress = arrNetPrinter[2], TypeDevice = arrNetPrinter[3] }));
                                    }
                                }
                            }
                        }
                    }
                }
                RaisePropertyChanged("IsEnableListNetPrintersMenu");
                _scanIsRuned = false;
                DispatcherHelper.CheckBeginInvokeOnUI(() => BtScan_content = "Сканировать");
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = true);
                if (!GetNetFromNameRemPC_isChecked)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = true);
                }
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "");
                DispatcherHelper.CheckBeginInvokeOnUI(() => UpdateSelectionPrintersOnRemPC());
                DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty());
                if (error)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => ShowErrorMessage(errorMessage));
                }
            });
        }
        private void StartCheckingAndGetDataListCompMode()
        {
            string net = "";
            // Запускаем новый поток
            Task.Factory.StartNew(() =>
            {
                // Усыпляем ненадолго поток
                Thread.Sleep(10);
                // Вызываем изменение текста лейбла в потоке UI
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Проверка указанных данных");
                DispatcherHelper.CheckBeginInvokeOnUI(() => BtScan_content = "Остановить сканирование");
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = false);

                if (InputDataIsValidListCompMode())
                {
                    string[] arrNet = Network.Split('.');
                    arrNet[0] = arrNet[0].Replace("_", "");
                    arrNet[1] = arrNet[1].Replace("_", "");
                    arrNet[2] = arrNet[2].Replace("_", "");
                    if (arrNet[0].Substring(0, 1) == "0")
                    {
                        arrNet[0] = arrNet[0].Substring(1);
                        if (arrNet[0].Substring(0, 1) == "0")
                        {
                            arrNet[0] = arrNet[0].Substring(1);
                        }
                    }
                    if (arrNet[1].Substring(0, 1) == "0")
                    {
                        if (!string.IsNullOrWhiteSpace(arrNet[1].Substring(1)))
                        {
                            arrNet[1] = arrNet[1].Substring(1);
                            if (arrNet[1].Substring(0, 1) == "0")
                            {
                                if (!string.IsNullOrWhiteSpace(arrNet[1].Substring(1)))
                                {
                                    arrNet[1] = arrNet[1].Substring(1);
                                }
                            }
                        }
                    }
                    if (arrNet[2].Substring(0, 1) == "0")
                    {
                        if (!string.IsNullOrWhiteSpace(arrNet[2].Substring(1)))
                        {
                            arrNet[2] = arrNet[2].Substring(1);
                            if (arrNet[2].Substring(0, 1) == "0")
                            {
                                if (!string.IsNullOrWhiteSpace(arrNet[2].Substring(1)))
                                {
                                    arrNet[2] = arrNet[2].Substring(1);
                                }
                            }
                        }
                    }
                    net = arrNet[0] + "." + arrNet[1] + "." + arrNet[2] + ".";
                    if (_scanIsRuned)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Сканирование сети на наличие сетевых принтеров");
                        DispatcherHelper.CheckBeginInvokeOnUI(() => AllSelected = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => _netPrinters.ClearAllData());
                        for (int i = FirstItem; i <= LastItem; i++)
                        {
                            if (!_scanIsRuned) break;
                            DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Сканирование сети на наличие сетевых принтеров " + net + i);
                            if (PingHost(net + i))
                            {
                                string printInfo = "";
                                if (App.mArgs != null && App.mArgs.Length > 0)
                                {
                                    if (App.mArgs[0].ToUpper() == "-DEBUG")
                                    {
                                        StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\snmperrorlog_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true);
                                        printInfo = GetSMNPPrinterInfo(net + i, _snmpConfig, sw);
                                        sw.Close();
                                    }
                                    else
                                    {
                                        printInfo = GetSMNPPrinterInfo(net + i, _snmpConfig);
                                    }
                                }
                                else
                                {
                                    printInfo = GetSMNPPrinterInfo(net + i, _snmpConfig);
                                }
                                if (!printInfo.Contains("error"))
                                {
                                    string[] arrNetPrinter = printInfo.Split(';');
                                    if (arrNetPrinter.GetUpperBound(0) > 0)
                                    {
                                        DispatcherHelper.CheckBeginInvokeOnUI(() => _netPrinters.addPrinter(new NetPrinters() { IsSelected = false, Name = arrNetPrinter[0], HostName = arrNetPrinter[1], IPAdress = arrNetPrinter[2], TypeDevice = arrNetPrinter[3] }));
                                    }
                                }
                            }
                        }
                    }
                }
                RaisePropertyChanged("IsEnableListNetPrintersMenu");
                _scanIsRuned = false;
                DispatcherHelper.CheckBeginInvokeOnUI(() => BtScan_content = "Сканировать");
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = true);
                if (!GetNetFromNameRemPC_isChecked)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = true);
                }
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "");
            });
        }
        private bool? AllItemSelected(ObservableCollection<NetPrinters> prn)
        {
            bool? result = false;
            int selectedItems = 0;
            prn.ToList().ForEach(x => { if (x.IsSelected) selectedItems++; });
            if (selectedItems > 0 && selectedItems == prn.Count)
                result = true;
            else if (selectedItems > 0 && selectedItems < prn.Count)
                result = null;
            return result;
        }
        private void UpdateAllSelectedCheckBox()
        {
            AllSelected = AllItemSelected(_snmpNetPrinters);
        }
        private string GetSMNPPrinterInfo(string ipAddress, SNMPConfig configData, StreamWriter logfile = null)
        {
            String resultStr = "";
            Dictionary<string, PrivacyProtocols> PrivacyProt = new Dictionary<string, PrivacyProtocols>();
            PrivacyProt.Add("DES", PrivacyProtocols.DES);
            PrivacyProt.Add("TRIPLEDES", PrivacyProtocols.TripleDES);
            PrivacyProt.Add("AES128", PrivacyProtocols.AES128);
            PrivacyProt.Add("AES192", PrivacyProtocols.AES192);
            PrivacyProt.Add("AES256", PrivacyProtocols.AES256);
            PrivacyProt.Add("NONE", PrivacyProtocols.None);

            Dictionary<string, AuthenticationDigests> AuthProt = new Dictionary<string, AuthenticationDigests>();
            AuthProt.Add("MD5", AuthenticationDigests.MD5);
            AuthProt.Add("SHA", AuthenticationDigests.SHA1);
            AuthProt.Add("NONE", AuthenticationDigests.None);

            // Make SNMP request
            AgentParameters param = new AgentParameters();
            SecureAgentParameters paramV3 = new SecureAgentParameters();
            SnmpV1Packet resultV1 = null;
            SnmpV2Packet resultV2 = null;
            SnmpV3Packet resultV3 = null;

            // Construct the agent address object
            IpAddress agent = new IpAddress(ipAddress);
            //--------------------------------------------------------------

            // Construct target
            //UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            UdpTarget target = new UdpTarget((IPAddress)agent, configData.PORT, configData.TIMEOUT, configData.RETRIES);
            //--------------------------------------------------------------

            // SNMP community name
            //OctetString community = new OctetString("public");
            OctetString community = new OctetString(configData.READCOMMUNITY);
            //--------------------------------------------------------------


            // Define agent parameters class
            if (configData.SNMPVER == "3")
            {
                try
                {
                    if (!target.Discovery(paramV3))
                    {
                        target.Close();
                        if (logfile != null)
                            logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error Discovery!!!");
                        return resultStr += "error Discovery!!!";
                    }
                }
                catch (Exception e)
                {
                    if (logfile != null)
                        logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error Discovery: " + e.Message);
                    return resultStr += "error Discovery: " + e.Message;
                }

                if (!string.IsNullOrWhiteSpace(configData.CONTEXTNAME))
                    paramV3.ContextName.Set(configData.CONTEXTNAME);

                if (configData.AUTHALGORITHM == "NONE" || string.IsNullOrWhiteSpace(configData.AUTHALGORITHM))
                {
                    paramV3.noAuthNoPriv(configData.USER);
                }
                else
                {
                    if (configData.PRIVACYALGORITHM == "NONE" || string.IsNullOrWhiteSpace(configData.PRIVACYALGORITHM))
                    {
                        paramV3.authNoPriv(configData.USER, AuthProt[configData.AUTHALGORITHM], configData.PASSAUTH);
                    }
                    else
                    {
                        paramV3.authPriv(
                            configData.USER,
                            AuthProt[configData.AUTHALGORITHM], configData.PASSAUTH,
                            PrivacyProt[configData.PRIVACYALGORITHM], configData.PASSPRIVACY);
                    }
                }
            }
            else
            {
                param = new AgentParameters(community);
                // Set SNMP version to 1 (or 2)
                if (configData.SNMPVER == "1")
                    param.Version = SnmpVersion.Ver1;
                if (configData.SNMPVER == "2")
                    param.Version = SnmpVersion.Ver2;
            }
            //--------------------------------------------------------------

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.2.1.25.3.2.1.3.1"); // DeviceDescription
            pdu.VbList.Add("1.3.6.1.2.1.1.5.0"); //HostName
            pdu.VbList.Add("1.3.6.1.2.1.1.2.0"); //DeviceType
            //---------------------------------------------------------------

            try
            {
                // Make SNMP request
                if (configData.SNMPVER == "1")
                {
                    resultV1 = (SnmpV1Packet)target.Request(pdu, param);
                }
                else if (configData.SNMPVER == "2")
                {
                    resultV2 = (SnmpV2Packet)target.Request(pdu, param);
                }
                else if (configData.SNMPVER == "3")
                {
                    resultV3 = (SnmpV3Packet)target.Request(pdu, paramV3);
                }
            }
            catch (Exception e)
            {
                if (logfile != null)
                    logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error: " + e.Message);
                return resultStr += "error:" + e.Message;
            }
            // If result is null then agent didn't reply or we couldn't parse the reply.
            #region SNMP V1
            if (configData.SNMPVER == "1")
            {
                if (resultV1 != null)
                {
                    // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                    if (resultV1.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        if (logfile != null)
                            logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error in SNMP reply. Error " + resultV1.Pdu.ErrorStatus + " index " + resultV1.Pdu.ErrorIndex);
                        resultStr += "error in SNMP reply. Error " + resultV1.Pdu.ErrorStatus + " index " + resultV1.Pdu.ErrorIndex;
                    }
                    else
                    {
                        // Reply variables are returned in the same order as they were added to the VbList
                        if (resultV1.Pdu.VbList[2].Value.ToString() == "1.3.6.1.4.1.11.2.3.9.1")
                            resultStr += resultV1.Pdu.VbList[0].Value.ToString() + ";" + resultV1.Pdu.VbList[1].Value.ToString() + ";" + ipAddress + ";" + "HPNetPrinter";
                        else
                            resultStr += resultV1.Pdu.VbList[0].Value.ToString() + ";" + resultV1.Pdu.VbList[1].Value.ToString() + ";" + ipAddress + ";" + "UnknownType";
                    }
                }
                else
                {
                    if (logfile != null)
                        logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error: " + "No response received from SNMP agent.");
                    resultStr += "error:" + "No response received from SNMP agent.";
                }
            }
            #endregion
            #region SNMP V2
            else if (configData.SNMPVER == "2")
            {
                if (resultV2 != null)
                {
                    // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                    if (resultV2.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        if (logfile != null)
                            logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error in SNMP reply. Error " + resultV2.Pdu.ErrorStatus + " index " + resultV2.Pdu.ErrorIndex);
                        resultStr += "error in SNMP reply. Error " + resultV2.Pdu.ErrorStatus + " index " + resultV2.Pdu.ErrorIndex;
                    }
                    else
                    {
                        // Reply variables are returned in the same order as they were added to the VbList
                        if (resultV2.Pdu.VbList[2].Value.ToString() == "1.3.6.1.4.1.11.2.3.9.1")
                            resultStr += resultV2.Pdu.VbList[0].Value.ToString() + ";" + resultV2.Pdu.VbList[1].Value.ToString() + ";" + ipAddress + ";" + "HPNetPrinter";
                        else
                            resultStr += resultV2.Pdu.VbList[0].Value.ToString() + ";" + resultV2.Pdu.VbList[1].Value.ToString() + ";" + ipAddress + ";" + "UnknownType";
                    }
                }
                else
                {
                    if (logfile != null)
                        logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + "error:" + "No response received from SNMP agent.");
                    resultStr += "error:" + "No response received from SNMP agent.";
                }
            }
            #endregion
            #region SNMP V3
            else if (configData.SNMPVER == "3")
            {
                if (resultV3.ScopedPdu.Type == PduType.Report)
                {
                    resultStr += "error SNMPv3 report: ";
                    foreach (Vb v in resultV3.ScopedPdu.VbList)
                    {
                        resultStr += v.Oid.ToString();
                    }
                    if (logfile != null)
                        logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + resultStr);
                }
                else
                {
                    if (resultV3.ScopedPdu.ErrorStatus == 0)
                    {
                        if (resultV3.ScopedPdu.VbList[2].Value.ToString() == "1.3.6.1.4.1.11.2.3.9.1")
                            resultStr += resultV3.ScopedPdu.VbList[0].Value.ToString() + ";" + resultV3.ScopedPdu.VbList[1].Value.ToString() + ";" + ipAddress + ";" + "HPNetPrinter";
                        else
                            resultStr += resultV3.ScopedPdu.VbList[0].Value.ToString() + ";" + resultV3.ScopedPdu.VbList[1].Value.ToString() + ";" + ipAddress + ";" + "UnknownType";
                    }
                    else
                    {
                        resultStr += "error: " + SnmpError.ErrorMessage(resultV3.ScopedPdu.ErrorStatus) + "\r\nerror status: " + resultV3.ScopedPdu.ErrorStatus + "\r\nerror index: " + resultV3.ScopedPdu.ErrorIndex;
                        if (logfile != null)
                            logfile.WriteLine(DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy") + " " + ipAddress + " " + resultStr);
                    }
                }
            }
            #endregion

            target.Close();
            return resultStr;
        }
        private Dictionary<string, string> GetSNMPConfig(string configFilePach)
        {
            Dictionary<string, string> configData = new Dictionary<string, string>();
            string configDataFull = "";
            try
            {
                using (StreamReader sr = new StreamReader(configFilePach))
                {
                    configDataFull = sr.ReadToEnd();
                    string[] configArr = configDataFull.Split('\n');
                    for (int i = 0; i < configArr.Length; i++)
                    {
                        if (!configArr[i].Trim().StartsWith("#"))
                        {
                            string[] conf = configArr[i].Trim().TrimEnd('\r').Split('=');
                            if (conf.Length > 1)
                                configData.Add(conf[0].ToUpper(), conf[1]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return configData;
        }
        private bool CheckConfig(Dictionary<string, string> configData, ref string errorMsg)
        {
            bool result = true;
            if (configData.ContainsKey("SNMPVER"))
            {
                if (!configData.ContainsKey("TIMEOUT"))
                {
                    errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр TIMEOUT (таймаут SNMP запроса)!!!";
                    return false;
                }
                else
                {
                    int j;
                    if (!Int32.TryParse(configData["TIMEOUT"], out j))
                    {
                        errorMsg = "Ошибка конфигурационного файла:\r\nЗначение параметра TIMEOUT должно быть цифровым и целочисленым!!!";
                        return false;
                    }
                }
                if (!configData.ContainsKey("RETRIES"))
                {
                    errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр RETRIES (количество SNMP повторов)!!!";
                    return false;
                }
                else
                {
                    int j;
                    if (!Int32.TryParse(configData["RETRIES"], out j))
                    {
                        errorMsg = "Ошибка конфигурационного файла:\r\nЗначение параметра RETRIES должно быть цифровым и целочисленым!!!";
                        return false;
                    }
                }
                if (!configData.ContainsKey("PORT"))
                {
                    errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр PORT (порт на устройстве для получения запросов)!!!";
                    return false;
                }
                else
                {
                    int j;
                    if (!Int32.TryParse(configData["PORT"], out j))
                    {
                        errorMsg = "Ошибка конфигурационного файла:\r\nЗначение параметра PORT должно быть цифровым и целочисленым!!!";
                        return false;
                    }
                }
                if (!configData.ContainsKey("READCOMMUNITY"))
                {
                    errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр READCOMMUNITY (Наименование группы чтения на устройствах)!!!";
                    return false;
                }
                if ((configData["SNMPVER"] != "1") && (configData["SNMPVER"] != "2") && (configData["SNMPVER"] != "3"))
                {
                    errorMsg = "Ошибка конфигурационного файла:\r\nВерсия SNMP протокола указана некоректно!!!";
                    result = false;
                }
                else
                {
                    if (configData["SNMPVER"] == "3")
                    {
                        if (!configData.ContainsKey("USER"))
                        {
                            errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр USER (пользователь для соединения с устройствами)!!!";
                            return false;
                        }
                        if (!configData.ContainsKey("AUTHALGORITHM"))
                        {
                            errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр AUTHALGORITHM (алгоритм проверки подлинности)!!!";
                            return false;
                        }
                        else
                        {
                            if (configData["AUTHALGORITHM"].ToUpper() != "MD5" &&
                                configData["AUTHALGORITHM"].ToUpper() != "SHA" &&
                                configData["AUTHALGORITHM"].ToUpper() != "NONE")
                            {
                                errorMsg = "Ошибка конфигурационного файла:\r\nНе верное значение параметра AUTHALGORITHM (алгоритм проверки подлинности)!!!";
                                return false;
                            }
                            if (configData["AUTHALGORITHM"].ToUpper() != "NONE" && !string.IsNullOrWhiteSpace(configData["AUTHALGORITHM"].ToUpper()))
                            {
                                if (!configData.ContainsKey("PASSAUTH"))
                                {
                                    errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр PASSAUTH (пароль к алгоритму проверки подлинности)!!!";
                                    return false;
                                }
                                if (!configData.ContainsKey("PRIVACYALGORITHM"))
                                {
                                    errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр PRIVACYALGORITHM (алгоритм безопасности)!!!";
                                    return false;
                                }
                                else
                                {
                                    if (configData["PRIVACYALGORITHM"].ToUpper() != "DES" &&
                                        configData["PRIVACYALGORITHM"].ToUpper() != "TRIPLEDES" &&
                                        configData["PRIVACYALGORITHM"].ToUpper() != "AES128" &&
                                        configData["PRIVACYALGORITHM"].ToUpper() != "AES192" &&
                                        configData["PRIVACYALGORITHM"].ToUpper() != "AES256" &&
                                        configData["PRIVACYALGORITHM"].ToUpper() != "NONE")
                                    {
                                        errorMsg = "Ошибка конфигурационного файла:\r\nНе верное значение параметра PRIVACYALGORITHM (алгоритм проверки подлинности)!!!";
                                        return false;
                                    }
                                    if (configData["PRIVACYALGORITHM"].ToUpper() != "NONE" && !string.IsNullOrWhiteSpace(configData["PRIVACYALGORITHM"].ToUpper()))
                                    {
                                        if (!configData.ContainsKey("PASSPRIVACY"))
                                        {
                                            errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр PASSPRIVACY (пароль к алгоритму безопасности)!!!";
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                errorMsg = "Ошибка конфигурационного файла:\r\nОтсутствует параметр SNMPVER (версия SNMP протокола)!!!";
                result = false;
            }
            return result;
        }
        private bool CopyDir(string sourcePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath))
            {
                MessageBox.Show("Не указан путь папки источника или назначения для копирования!!");
                return false;
            }
            DirectoryInfo source = new DirectoryInfo(sourcePath);
            DirectoryInfo target = new DirectoryInfo(targetPath);

            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                MessageBox.Show("Папка источник и папка назначения совпадают!!");
                return false;
            }
            try
            {
                // Check if the target directory exists, if not, create it.
                if (Directory.Exists(target.FullName) == false)
                {
                    Directory.CreateDirectory(target.FullName);
                }

                // Copy each file into it's new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyDir(diSourceSubDir.FullName, nextTargetSubDir.FullName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }
        private bool CopyDir(string sourcePath, string targetPath, ref string errorStr)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetPath))
            {
                errorStr += "Ошибка копирования: Не указан путь папки источника или назначения для копирования!!\r\n";
                return false;
            }
            DirectoryInfo source = new DirectoryInfo(sourcePath);
            DirectoryInfo target = new DirectoryInfo(targetPath);

            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                errorStr += "Ошибка копирования: Папка источник и папка назначения совпадают!!\r\n";
                return false;
            }
            try
            {
                // Check if the target directory exists, if not, create it.
                if (Directory.Exists(target.FullName) == false)
                {
                    Directory.CreateDirectory(target.FullName);
                }

                // Copy each file into it's new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyDir(diSourceSubDir.FullName, nextTargetSubDir.FullName);
                }
            }
            catch (Exception e)
            {
                errorStr += e.ToString() + "\r\n";
                return false;
            }
            return true;
        }
        private int?[] RunProcessOnRemoteHost(string remoteHost, string cmdCommand)
        {
            int?[] result = new int?[2]; // result[0] - возвращаемое значение результата запуска процесса, result[1] - Id запущенного процесса, если процесс был запущен, иначе null
            Console.WriteLine("cmd command: " + cmdCommand);
            try
            {
                ConnectionOptions connOptions = new ConnectionOptions();// new ConnectionOptions {Username = username,Password = password}; - для задания логина и пароля пользователя от чьего имени запустить процесс
                connOptions.Impersonation = ImpersonationLevel.Impersonate;
                connOptions.EnablePrivileges = true;
                ManagementScope manScope = new ManagementScope(String.Format(@"\\{0}\ROOT\CIMV2", remoteHost), connOptions);
                manScope.Connect();
                ObjectGetOptions objectGetOptions = new ObjectGetOptions();
                ManagementPath managementPath = new ManagementPath("Win32_Process");
                ManagementClass processClass = new ManagementClass(manScope, managementPath, objectGetOptions);
                ManagementBaseObject inParams = processClass.GetMethodParameters("Create");
                inParams["CommandLine"] = cmdCommand;
                ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
                result[0] = Convert.ToInt32(outParams["returnValue"]);
                if (!string.IsNullOrEmpty(outParams["processId"].ToString()))
                    result[1] = Convert.ToInt32(outParams["processId"]);
                else
                    result[1] = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error1 " + e.Message);
                result[0] = -1;
                result[1] = null;
            }
            return result;
        }
        private Dictionary<string, string> GetConfigData(string configFilePach)
        {
            Dictionary<string, string> configData = new Dictionary<string, string>();
            string configDataFull = "";

            try
            {
                using (StreamReader sr = new StreamReader(configFilePach))
                {
                    configDataFull = sr.ReadToEnd();
                    string[] configArr = configDataFull.Split('\n');
                    for (int i = 0; i < configArr.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(configArr[i]))
                        {
                            string[] conf = configArr[i].Split('=');
                            configData.Add(conf[0], conf[1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка чтения файла конфигурации драйверов: " + e.Message);
                configData = null;
            }
            return configData;
        }
        private Dictionary<string, string> GetConfigData(string configFilePach, ref string errorStr)
        {
            Dictionary<string, string> configData = new Dictionary<string, string>();
            string configDataFull = "";

            try
            {
                using (StreamReader sr = new StreamReader(configFilePach))
                {
                    configDataFull = sr.ReadToEnd();
                    string[] configArr = configDataFull.Split('\n');
                    for (int i = 0; i < configArr.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(configArr[i]))
                        {
                            string[] conf = configArr[i].Split('=');
                            configData.Add(conf[0], conf[1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorStr += "Ошибка чтения файла конфигурации драйверов " + configFilePach + ":\r\n" + e.Message;
                configData = null;
            }
            return configData;
        }
        private Dictionary<string, string> GetTypesNetPrinters()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string configDataFull = "";

            try
            {
                using (StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\typesnetprinters.cfg"))
                {
                    configDataFull = sr.ReadToEnd();
                    string[] configArr = configDataFull.Split('\n');
                    for (int i = 0; i < configArr.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(configArr[i]))
                        {
                            string[] conf = configArr[i].Split('=');
                            result.Add(conf[0], conf[1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка чтения файла типов сетевых принтеров: " + e.Message);
                result = null;
            }

            return result;
        }
        private Dictionary<string, ObservableCollection<InstalledPrinters>> GetInstalledPrintersOnRemotePC(string host, ref bool err, ref string errMsg)
        {
            Dictionary<string, ObservableCollection<InstalledPrinters>> result = new Dictionary<string, ObservableCollection<InstalledPrinters>>();
            Dictionary<string, string> defaultPrinterUsers = new Dictionary<string, string>();
            RegistryKey printKeyLM;
            RegistryKey printKeyU;
            string user = "";
            string[] prn = new string[3];
            try
            {
                printKeyLM = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, host);
                printKeyU = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, host);
                String[] namesU = printKeyU.GetSubKeyNames();
                String[] namesLM = printKeyLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers").GetSubKeyNames();
                try
                {
                    #region Поиск активных пользователей в реестре и получение сетевых принтеров этих пользователей
                    foreach (String s in namesU)
                    {
                        String[] key = printKeyU.OpenSubKey(s).GetSubKeyNames();
                        foreach (String k in key)
                        {
                            //Если писутствует ветка Volatile Environment значит это активный пользователь
                            if (k == "Volatile Environment")
                            {
                                //Получение логина пользователя
                                RegistryKey CurUserKey = printKeyU.OpenSubKey(s + "\\" + k);
                                if (CurUserKey != null)
                                {
                                    var name = CurUserKey.GetValue("USERNAME");
                                    if (name != null)
                                    {
                                        user = (string)name;
                                        result.Add((string)name, new ObservableCollection<InstalledPrinters>());
                                    }
                                }
                                else
                                {
                                    user = "Unknow";
                                    result.Add("Unknow", new ObservableCollection<InstalledPrinters>());
                                }
                                //Получение принтера поумолчанию для текущего пользователя
                                RegistryKey defaultPrinterKey = printKeyU.OpenSubKey(s + @"\Software\Microsoft\Windows NT\CurrentVersion\Windows");
                                if (defaultPrinterKey != null)
                                {
                                    var printerName = (object)null;
                                    try
                                    {
                                        printerName = defaultPrinterKey.GetValue("Device");
                                    }
                                    catch (Exception)
                                    { }
                                    if (printerName != null)
                                    {
                                        defaultPrinterUsers.Add(user, ((string)printerName).Split(',')[0]);
                                    }
                                }
                                //Получение сетевых принтеров пользователя
                                foreach (String p in key)
                                {
                                    if (p == "Printers")
                                    {
                                        String[] netPrinters = printKeyU.OpenSubKey(s + "\\" + p + "\\" + "Connections").GetSubKeyNames();
                                        if (netPrinters.Length > 0)
                                        {
                                            foreach (String printer in netPrinters)
                                            {
                                                string[] strArray = printer.Split(',');
                                                if (defaultPrinterUsers.ContainsKey(user) && (defaultPrinterUsers[user] == @"\\" + strArray[2] + @"\" + strArray[3]))
                                                {
                                                    result[user].Add(new InstalledPrinters()
                                                    {
                                                        Name = @"\\" + strArray[2] + @"\" + strArray[3],
                                                        Port = strArray[2],
                                                        RegKey = @"HKEY_USERS\" + s + "\\" + p + "\\" + "Connections" + "\\" + printer,
                                                        IsDefault = new BitmapImage(new Uri(@"/RemoteSetupPrinters;component/Resources/defaultPrinter.ico", UriKind.RelativeOrAbsolute))
                                                    });
                                                }
                                                else
                                                {
                                                    result[user].Add(new InstalledPrinters()
                                                    {
                                                        Name = @"\\" + strArray[2] + @"\" + strArray[3],
                                                        Port = strArray[2],
                                                        RegKey = @"HKEY_USERS\" + s + "\\" + p + "\\" + "Connections" + "\\" + printer
                                                    });
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    #endregion
                    //Если не найден ни один активный пользователь создаём пустого пользователя.
                    if (result.Keys.Count <= 0)
                    {
                        user = "Unknow";
                        result.Add("Unknow", new ObservableCollection<InstalledPrinters>());
                    }
                    #region Получение локально установленных принтеров и добавление их активным пользователям
                    foreach (String p in namesLM)
                    {
                        //Имя принтера
                        prn[0] = p;
                        //Получение порта принтера
                        RegistryKey LocalMachineKey = printKeyLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers\" + p);
                        var port = LocalMachineKey.GetValue("Port");
                        if (port != null)
                        {
                            prn[1] = (string)port;
                        }
                        else
                        {
                            prn[1] = "";
                        }
                        //Получение IP адреса принтера
                        RegistryKey DriverKey = printKeyLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Print\Printers\" + p + "\\PrinterDriverData");
                        if (DriverKey != null)
                        {
                            var ipadress = DriverKey.GetValue("HPEWSIPAddress");
                            if (ipadress != null)
                            {
                                prn[2] = ((string)ipadress).Split(',').GetValue(0).ToString();
                            }
                            else
                            {
                                prn[2] = " ";
                            }
                        }
                        else
                        {
                            prn[2] = " ";
                        }
                        //Добавление принтера активным пользователям
                        foreach (string key in result.Keys)
                        {
                            if (defaultPrinterUsers.ContainsKey(user) && (defaultPrinterUsers[key] == prn[0]))
                            {
                                result[key].Add(new InstalledPrinters()
                                {
                                    Name = prn[0],
                                    Port = prn[1],
                                    IPAdress = prn[2],
                                    IsDefault = new BitmapImage(new Uri(@"/RemoteSetupPrinters;component/Resources/defaultPrinter.ico", UriKind.RelativeOrAbsolute))
                                });
                            }
                            else
                            {
                                result[key].Add(new InstalledPrinters()
                                {
                                    Name = prn[0],
                                    Port = prn[1],
                                    IPAdress = prn[2]
                                });
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    //Если при получении пользователей и принтеров произошла ошибка сгенерировать исключение
                    err = true;
                    errMsg += ex.Message;
                }
            }
            catch (Exception e)
            {
                //Если при обращении к реестру удаленного компьютера произошла ошибка сгенерировать исключение
                err = true;
                errMsg += "Не удалось получить информацию о принтерах\r\nиз реестра удаленного компьютера.\r\nВозможно удаленный компьютер ещё не полностью загрузился.\r\nПопробуйте повторить сканирование позднее.\r\nОшибка: " + e.Message;
            }
            return result;
        }
        private void SetUpSelectedPrinters()
        {
            #region Проверка доступности удаленного компьютера и изменений поля с именем хоста
            if (!string.IsNullOrWhiteSpace(NameHosts))
            {
                // Проверка изменений в поле удаленного компьютера
                if (PrevNameHosts != NameHosts)
                {
                    MessageBox.Show("Наименование удаленного компьютера было изменено.\nПовторите сканирование!");
                    return;
                }
                // Проверка доступности удаленного компьютера перед подключением
                if (!PingHost(NameHosts))
                {
                    MessageBox.Show("Удаленный компьютер не доступен!!");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Не указан удаленный компьютер!!");
                return;
            }
            #endregion
            const string quote = "\"";
            bool err = false;
            //string defaultPrinter = "";
            string logRunedProcess = "";
            string errorLog = "";
            ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(Application.Current.MainWindow, "Установка принтеров", (bw, we) =>
            {
                #region Получение списка выбранных принтеров
                //-----------------------------------------------------------------------------------------
                //Получение списка выбранных принтеров
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nФормирование списка принтеров...", NameHosts))
                    return;
                Thread.Sleep(250);
                List<NetPrinters> selectedPrinters = new List<NetPrinters>();
                for (int i = 0; i < _snmpNetPrinters.Count; i++)
                {
                    if (_snmpNetPrinters[i].IsSelected)
                        selectedPrinters.Add(_snmpNetPrinters[i]);
                }
                if (selectedPrinters.Count <= 0)
                {
                    errorLog += "Ошибка формирования списка принтеров: Не выбран ни один из принтеров\r\n";
                    return;
                }
                #endregion
                #region Сравнение списка выбранных принтеров со списком установленных на удаленном компьютере по IP, если совпадают не устанавливать.
                // Сравнение списка выбранных принтеров со списком установленных на удаленном компьютере по IP, 
                // Если один из выбранных принтеров уже установлен на удаленном компьютере то данный принтер удаляется из списка выбранных
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nПроверка установленных принтеров", NameHosts))
                    return;
                Thread.Sleep(250);
                try
                {
                    for (int i = 0; i < _installedPrintersOnRemPC.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(_installedPrintersOnRemPC[i].IPAdress))
                        {
                            for (int j = 0; j < selectedPrinters.Count; j++)
                            {
                                if (selectedPrinters[j].IPAdress == _installedPrintersOnRemPC[i].IPAdress)
                                {
                                    //MessageBox.Show("Принтер " + selectedPrinters[j].Name + " " + selectedPrinters[j].IPAdress + " уже установлен на удаленном компьюте. Подключение данного принтера не будет выполненно.");
                                    logRunedProcess += "#info\r\nПринтер " + selectedPrinters[j].Name + "_" + selectedPrinters[j].IPAdress + " уже установлен на удаленном компьютере.\r\nПодключение данного принтера не будет выполненно.\r\n\r\n";
                                    selectedPrinters.RemoveAt(j);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    errorLog += "Ошибка сравнения выбранных и установленных принтеров:\r\n" + e.ToString();
                    return;
                }
                if (selectedPrinters.Count <= 0)
                {
                    logRunedProcess += "#info\r\nНет принтеров для установки\r\n\r\n";
                    return;
                }
                #endregion
                #region Проверка доступности драйверов для выбранных принтеров
                //-----------------------------------------------------------------------------------------
                // Проверка доступности драйверов для выбранных принтеров
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nПроверка драйверов...", NameHosts))
                    return;
                Thread.Sleep(250);
                List<string> driversToCopy = new List<string>();
                try
                {
                    string dirPath = Directory.GetCurrentDirectory() + "\\Drivers";
                    List<string> driversList = new List<string>(Directory.EnumerateDirectories(dirPath));
                    int i = 0;
                    bool findFlag, exists;
                    while (i < selectedPrinters.Count)
                    {
                        findFlag = false;
                        exists = false;
                        for (int j = 0; j < driversList.Count; j++)
                        {
                            if (selectedPrinters[i].Name == driversList[j].Substring(driversList[j].LastIndexOf("\\") + 1))
                            {
                                findFlag = true;
                                driversToCopy.ForEach(x => { if (x == driversList[j]) exists = true; });
                                if (!exists) driversToCopy.Add(driversList[j]);
                                break;
                            }
                        }
                        if (findFlag)
                        {
                            i++;
                        }
                        else
                        {
                            logRunedProcess += "#info\r\nДрайвера на принтер " + selectedPrinters[i].Name + " " + selectedPrinters[i].IPAdress + " отсутствуют!!!\r\nПодключение данного принтера не будет выполненно.\r\n\r\n";
                            selectedPrinters.RemoveAt(i);
                        }
                    }
                }
                catch (UnauthorizedAccessException UAEx)
                {
                    errorLog += UAEx.Message + "\r\n";
                    err = true;
                }
                catch (PathTooLongException PathEx)
                {
                    errorLog += PathEx.Message + "\r\n";
                    err = true;
                }
                catch (Exception e)
                {
                    errorLog += e.ToString() + "\r\n";
                    err = true;
                }
                if (selectedPrinters.Count <= 0)
                {
                    logRunedProcess += "#info\r\nНет принтеров для установки\r\n\r\n";
                    return;
                }
                if (err)
                {
                    return;
                }
                #endregion
                #region Копирование драйверов на удаленный компьютер
                //-----------------------------------------------------------------------------------------
                // Копирование драйверов на удаленный компьютер
                Dictionary<string, string> configDriversFiles = new Dictionary<string, string>();
                if (Directory.Exists(@"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\"))
                {
                    try
                    {
                        Directory.Delete(@"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\", true);
                    }
                    catch (Exception exp)
                    {
                        errorLog += "Ошибка удаления существующей дирректории:\r\n" + exp.ToString() + "\r\n";
                        err = true;
                        return;
                    }
                }
                for (int i = 0; i < driversToCopy.Count; i++)
                {
                    if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nКопирование драйверов: {1}", NameHosts, driversToCopy[i].Substring(driversToCopy[i].LastIndexOf("\\") + 1)))
                        return;
                    Thread.Sleep(1);
                    if (!CopyDir(driversToCopy[i], @"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\" + driversToCopy[i].Substring(driversToCopy[i].LastIndexOf("\\") + 1), ref errorLog))
                    {
                        err = true;
                        return;
                    }
                    configDriversFiles.Add(driversToCopy[i].Substring(driversToCopy[i].LastIndexOf("\\") + 1), driversToCopy[i]);
                }
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nКопирование prnport.vbs", NameHosts))
                    return;
                Thread.Sleep(1);
                try
                {
                    File.Copy(Directory.GetCurrentDirectory() + @"\Drivers\prnport.vbs", @"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\prnport.vbs", true);
                }
                catch (Exception e)
                {
                    errorLog += e.ToString() + "\r\n";
                    err = true;
                    return;
                }
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nКопирование prncnfg.vbs", NameHosts))
                    return;
                Thread.Sleep(1);
                try
                {
                    File.Copy(Directory.GetCurrentDirectory() + @"\Drivers\prncnfg.vbs", @"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\prncnfg.vbs", true);
                }
                catch (Exception e)
                {
                    errorLog += e.ToString() + "\r\n";
                    err = true;
                    return;
                }
                if (err)
                {
                    return;
                }
                #endregion
                //-----------------------------------------------------------------------------------------
                // Подключение принтеров
                #region Создание .bat файла на удаленном компьютере
                // Создание .bat файла на удаленном компьютере
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nФормирование пакетного файла...", NameHosts))
                    return;
                Thread.Sleep(250);
                string sBatFile = @"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\setupprn.bat";
                try
                {
                    if (File.Exists(sBatFile))
                    {
                        File.Delete(sBatFile);
                    }
                    StreamWriter sw = new StreamWriter(sBatFile, false, Encoding.GetEncoding("cp866"));
                    sw.WriteLine(@"@echo off");
                    for (int i = 0; i < selectedPrinters.Count; i++)
                    {
                        Dictionary<string, string> configData = GetConfigData(configDriversFiles[selectedPrinters[i].Name] + "\\configRSP.cfg");
                        if (configData == null) return;
                        sw.WriteLine("echo #connect");
                        sw.WriteLine("echo Подключение принтера " + configData["printerName"].TrimEnd('\r', '\n') + "__" + selectedPrinters[i].IPAdress + " на ПК " + NameHosts);
                        sw.WriteLine("echo =Создание порта для подключения принтера " + configData["printerName"].TrimEnd('\r', '\n') + "_" + selectedPrinters[i].IPAdress);
                        string command = @"C:\windows\system32\cscript.exe C:\Windows\Temp\Drivers\prnport.vbs -a -r IP_" + selectedPrinters[i].IPAdress + @" -h " + selectedPrinters[i].IPAdress + @" -o raw -n 9100";
                        sw.WriteLine(command);
                        sw.WriteLine("echo Завершено.\necho.");
                        sw.WriteLine("echo Подключение принтера " + configData["printerName"].TrimEnd('\r', '\n') + "_" + selectedPrinters[i].IPAdress);
                        command = @"c:\windows\system32\rundll32.exe printui.dll,PrintUIEntry /q /if /b " + configData["printerName"].TrimEnd('\r', '\n') + "_" + selectedPrinters[i].IPAdress + @" /f " + quote + @"C:\Windows\Temp\Drivers\" + selectedPrinters[i].Name + "\\" + selectedPrinters[i].Name.Replace(" ", "") + ".inf" + quote + @" /r IP_" + selectedPrinters[i].IPAdress + @" /m " + quote + configData["nameModelDriver"].TrimEnd('\r', '\n') + quote + @" /z";
                        sw.WriteLine(command);
                        sw.WriteLine("echo Завершено.\necho.");
                        sw.WriteLine("echo Проверка подключения принтера " + configData["printerName"].TrimEnd('\r', '\n') + "_"  + selectedPrinters[i].IPAdress);
                        command = @"C:\windows\system32\cscript.exe C:\Windows\Temp\Drivers\prncnfg.vbs -g -p " + quote + configData["printerName"].TrimEnd('\r', '\n') + "_" + selectedPrinters[i].IPAdress + quote;
                        sw.WriteLine(command);
                        sw.WriteLine("echo Завершено.\n");
                    }
                    sw.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    errorLog += "Ошибка при создании .bat файла на удаденном компьютере " + NameHosts + ":\r\n" + ex.Message;
                    err = true;
                }
                if (err)
                {
                    // Если при создании .bat файла произошла ошибка, завершить обработку.
                    return;
                }
                #endregion
                #region Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                //-----------------------------------------------------------------------------------------
                // Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nПодключение принтеров...", NameHosts))
                    return;
                Thread.Sleep(250);
                bool procIsRuned = false;
                string runCommand = @"cmd.exe /c C:\Windows\Temp\Drivers\setupprn.bat > C:\Windows\Temp\Drivers\logruned.log 2>&1";
                int?[] res = RunProcessOnRemoteHost(NameHosts, runCommand);
                if (res[1] != null)
                {
                    // процесс успешно запущен
                    #region Ожидание завершения подключения принтера
                    //ожидание выполнения подключения принтера до тех пор пока существует процесс с указанным ID
                    procIsRuned = true;
                    try
                    {
                        ManagementScope scope = new ManagementScope("\\\\" + NameHosts + "\\root\\cimv2");
                        scope.Connect();
                        ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process WHERE ProcessID = " + res[1]);
                        while (procIsRuned)
                        {
                            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                            if (searcher.Get().Count <= 0)
                            {
                                procIsRuned = false;
                            }
                            Thread.Sleep(100);
                        }
                    }
                    catch (ManagementException e)
                    {
                        errorLog += "Произошла ошибка во время запроса данных WMI с удаленного компьютера " + NameHosts + ":\r\n" + e.Message;
                        err = true;
                    }
                    catch (System.UnauthorizedAccessException unauthorizedErr)
                    {
                        errorLog += "Ошибка соединения с удаленным компьютером " + NameHosts + ":\r\n" + unauthorizedErr.Message;
                        err = true;
                    }
                    if (err)
                    {
                        return;
                    }
                    #endregion
                    #region Чтение лог файла
                    // по окончанию выполнения процесса считать лог работы и вывести на экран.
                    if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nЧтение лог файла...", NameHosts))
                        return;
                    Thread.Sleep(250);
                    try
                    {
                        using (StreamReader sr = new StreamReader(@"\\" + NameHosts + @"\C$\Windows\Temp\Drivers\logruned.log", Encoding.GetEncoding("cp866")))
                        {
                            logRunedProcess = sr.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        errorLog += "Не удалось прочитать файл лога с удаленного компьютера " + NameHosts + ":\r\n" + e.Message + "\r\n";
                        err = true;
                    }
                    if (err)
                    {
                        return;
                    }
                    #endregion
                    #region Удаление скопированных файлов
                    if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nУдаление скопированных файлов...", NameHosts))
                        return;
                    Thread.Sleep(1);
                    try
                    {
                        Directory.Delete(@"\\" + NameHosts + @"\C$\Windows\Temp\Drivers", true);
                    }
                    catch (Exception e)
                    {
                        errorLog += "Ошибка удаления папки на удаленном компьютере: " + e.Message + "\r\n";
                    }
                    if (err)
                    {
                        return;
                    }
                    #endregion
                    #region Обновление списка установленных принтеров
                    // Обновление списка установленных принтеров
                    if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, "Обработка {0}...\nОбновление списка установленных принтеров...", NameHosts))
                        return;
                    Thread.Sleep(1);
                    if (!err)
                    {
                        string errStr = "";
                        _installedPrintersOnRemPCWithUsers = null;
                        DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrintersOnRemPCWithUsers = GetInstalledPrintersOnRemotePC(NameHosts, ref err, ref errStr));
                        while (_installedPrintersOnRemPCWithUsers == null)
                        {
                            Thread.Sleep(500);
                        }
                        if (err)
                        {
                            errorLog += "Ошибка получения информации об установленных принтерах:\r\n" + errStr;
                        }
                        if (!err)
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() => UserOnRemPC = _installedPrintersOnRemPCWithUsers.Keys.ToList());
                            DispatcherHelper.CheckBeginInvokeOnUI(() => SelectedUserOnRemPC = UserOnRemPC[0]);
                            DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrinters.updateData(_installedPrintersOnRemPCWithUsers[UserOnRemPC[0]]));
                            DispatcherHelper.CheckBeginInvokeOnUI(() => UpdateSelectionPrintersOnRemPC());
                            DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty());
                        }
                    }
                    //-----------------------------------------------
                    #endregion
                }
                else
                {
                    // ошибка запуска процесса
                    err = true;
                    switch (res[0])
                    {
                        case 2:
                            errorLog += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", отказанно в доступе!!";
                            break;
                        case 3:
                            errorLog += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", недостаточно прав!!";
                            break;
                        case 8:
                            errorLog += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", неизвестная ошибка!!";
                            break;
                        case 9:
                            errorLog += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", путь не найден!!";
                            break;
                        case 21:
                            errorLog += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", ошибочные параметры!!";
                            break;
                        default:
                            errorLog += "Не удалось запустить процесс на удаленном компьютере " + NameHosts;
                            break;
                    }
                }
                if (err)
                {
                    return;
                }
                #endregion
                //-----------------------------------------------------------------------------------------
                ProgressDialog.ProgressDialog.CheckForPendingCancellation(bw, we);
            }, ProgressDialogSettings.WithSubLabelAndCancel);

            if (result.Cancelled){
                ReportWindow.ReportWindow _rW = new ReportWindow.ReportWindow("error\r\n" + errorLog + "\r\n" + logRunedProcess);
                _rW.ShowDialog();
            }else if (result.OperationFailed){
                MessageBox.Show("Ошибка!!!.");
            }else{
                ReportWindow.ReportWindow _rW = new ReportWindow.ReportWindow("error\r\n" + errorLog + "\r\n" + logRunedProcess);
                _rW.offlineExpander.Visibility = Visibility.Collapsed;
                _rW.ShowDialog();
            }
        }
        private void SetUpSelectedPrintersListCompMode()
        {
            if (string.IsNullOrWhiteSpace(NameHosts))
            {
                MessageBox.Show("Не указан список удаленных компьютеров!!");
                return;
            }
            const string quote = "\"";
            bool findFlag, exists;
            bool err = false;
            string offlinePC = "";
            string logRunedProcess = "";
            string errorLog = "";
            ProgressDialogResult result = ProgressDialog.ProgressDialog.Execute(Application.Current.MainWindow, "Установка принтеров", (bw, we) =>
            {
                // Преобразование строки удаленных компьютеров  в массив
                string listRemPC = NameHosts.Replace(" ", "");
                string[] arrRemPC = listRemPC.Split(',');

                #region Получение списка выбранных принтеров
                //-----------------------------------------------------------------------------------------
                //Получение списка выбранных принтеров
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, 0, "Формирование списка принтеров..."))
                    return;
                Thread.Sleep(250);
                List<NetPrinters> selectedPrinters = new List<NetPrinters>();
                for (int i = 0; i < _snmpNetPrinters.Count; i++)
                {
                    if (_snmpNetPrinters[i].IsSelected)
                        selectedPrinters.Add(_snmpNetPrinters[i]);
                }
                if (selectedPrinters.Count <= 0)
                {
                    errorLog += "Ошибка формирования списка принтеров: Не выбран ни один из принтеров\r\n";
                    return;
                }
                #endregion
                #region Проверка доступности драйверов для выбранных принтеров
                //-----------------------------------------------------------------------------------------
                // Проверка доступности драйверов для выбранных принтеров
                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, 0, "Проверка драйверов..."))
                    return;
                Thread.Sleep(250);
                List<string> driversToCopy = new List<string>();
                try
                {
                    string dirPath = Directory.GetCurrentDirectory() + "\\Drivers";
                    List<string> driversList = new List<string>(Directory.EnumerateDirectories(dirPath));
                    int i = 0;
                    while (i < selectedPrinters.Count)
                    {
                        findFlag = false;
                        exists = false;
                        for (int j = 0; j < driversList.Count; j++)
                        {
                            if (selectedPrinters[i].Name == driversList[j].Substring(driversList[j].LastIndexOf("\\") + 1))
                            {
                                findFlag = true;
                                driversToCopy.ForEach(x => { if (x == driversList[j]) exists = true; });
                                if (!exists) driversToCopy.Add(driversList[j]);
                                break;
                            }
                        }
                        if (findFlag)
                        {
                            i++;
                        }
                        else
                        {
                            logRunedProcess += "#info\r\nДрайвера на принтер " + selectedPrinters[i].Name + " " + selectedPrinters[i].IPAdress + " отсутствуют!!!\r\nПодключение данного принтера не будет выполненно.\r\n\r\n";
                            selectedPrinters.RemoveAt(i);
                        }
                    }
                }
                catch (UnauthorizedAccessException UAEx)
                {
                    errorLog += UAEx.Message + "\r\n";
                    err = true;
                }
                catch (PathTooLongException PathEx)
                {
                    errorLog += PathEx.Message + "\r\n";
                    err = true;
                }
                catch (Exception e)
                {
                    errorLog += e.ToString() + "\r\n";
                    err = true;
                }
                if (selectedPrinters.Count <= 0)
                {
                    logRunedProcess += "#info\r\nНет принтеров для установки\r\n\r\n";
                    return;
                }
                if (err)
                {
                    return;
                }
                #endregion
                #region Обработка списка удаленных компьютеров
                //-----------------------------------------------------------------------------------------
                //Обработка списка удаленных компьютеров
                for (int i = 0; i < arrRemPC.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(arrRemPC[i]))
                    {
                        #region Проверка доступности удаленного компьютера в сети
                        // Проверка доступности удаленного компьютера в сети
                        if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i + 1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nПроверка доступности в сети", i + 1, arrRemPC[i], arrRemPC.Length))
                            return;
                        Thread.Sleep(250);
                        if (PingHost(arrRemPC[i]))
                        {
                            // Копирование списка выбранных принтеров для дальнейшей обработки
                            List<NetPrinters> printersForInstall = selectedPrinters;
                            #region Проверка установленных принтеров на удаленном компьютере
                            //-----------------------------------------------------------------------------------------
                            // Получение IP адресов принтеров установленных на уделенном компьютере и сравнение их со списком адресов выбранных принтеров
                            // Если один из выбранных принтеров уже установлен на удаленном компьютере то данный принтер удаляется из списка копирунмых
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i+1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nПроверка установленных принтеров", i+1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(250);
                            string installedPrintersStr = "";
                            try
                            {
                                installedPrintersStr = GetPrintersInfoFromRemotePC(arrRemPC[i]);
                            }
                            catch (SecurityException e)
                            {
                                errorLog += "Ошибка доступа к удаленному компьютеру " + arrRemPC[i] + ": " + e.Message + "\r\n";
                                err = true;
                            }
                            if (!err)
                            {
                                if (!string.IsNullOrWhiteSpace(installedPrintersStr))
                                {
                                    String[] arrPrinters = installedPrintersStr.Split('|');
                                    for (int j = 0; j < arrPrinters.Length - 1; j++)
                                    {
                                        string[] arrPrinter = arrPrinters[j].Split(';');
                                        if (arrPrinter.GetUpperBound(0) > 0)
                                        {
                                            if (!string.IsNullOrWhiteSpace(arrPrinter[2]))
                                            {
                                                int n = 0;
                                                while (n < printersForInstall.Count)
                                                {
                                                    if (printersForInstall[n].IPAdress == arrPrinter[2])
                                                    {
                                                        logRunedProcess += "#info\r\nПринтер " + printersForInstall[n].Name + "_" + printersForInstall[n].IPAdress + " уже установлен на компьютере " + arrRemPC[i] + ".\r\nПодключение данного принтера не будет выполненно.\r\n\r\n";
                                                        printersForInstall.RemoveAt(n);
                                                    }
                                                    else
                                                    {
                                                        n++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Если при доступе к удаленному компьютеру произошла ошибка, перейти к следующему удаленному компьютеру.
                                continue;
                            }
                            if (printersForInstall.Count <= 0)
                            {
                                logRunedProcess += "#info\r\nНет принтеров для установки\r\n\r\n";
                                continue;
                            }
                            #endregion
                            //-----------------------------------------------------------------------------------------
                            // Копирование списка драйверов выбранных принтеров для дальнейшей обработки 
                            List<string> driversToCopyForThisHost = driversToCopy;
                            #region Удаление ненужных драйверов из списка копируемых
                            //-----------------------------------------------------------------------------------------
                            // Удаление ненужных драйверов из списка копируемых
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i+1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nОбработка драйверов...", i+1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(250);
                            int x = 0;
                            while (x < driversToCopyForThisHost.Count)
                            {
                                findFlag = false;
                                for (int z = 0; z < printersForInstall.Count; z++)
                                {
                                    if (printersForInstall[z].Name == driversToCopyForThisHost[x].Substring(driversToCopyForThisHost[x].LastIndexOf("\\") + 1))
                                    {
                                        findFlag = true;
                                        break;
                                    }
                                }
                                if (findFlag)
                                {
                                    x++;
                                }
                                else
                                {
                                    // Для данного драйвера отсутствует принтер для установки, драйвер удаляется из списка копируемых
                                    driversToCopyForThisHost.RemoveAt(x);
                                }
                            }
                            #endregion
                            #region Копирование драйверов на удаленный компьютер
                            //-----------------------------------------------------------------------------------------
                            // Копирование драйверов на удаленный компьютер
                            Dictionary<string, string> configDriversFiles = new Dictionary<string, string>();
                            for (int k = 0; k < driversToCopyForThisHost.Count; k++)
                            {
                                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i+1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{3}...\nКопирование драйверов: {2}", i+1, arrRemPC[i], driversToCopyForThisHost[k].Substring(driversToCopyForThisHost[k].LastIndexOf("\\") + 1), arrRemPC.Length))
                                    return;
                                Thread.Sleep(1);
                                if (!CopyDir(driversToCopyForThisHost[k], @"\\" + arrRemPC[i] + @"\C$\Windows\Temp\Drivers\" + driversToCopyForThisHost[k].Substring(driversToCopyForThisHost[k].LastIndexOf("\\") + 1), ref errorLog))
                                {
                                    err = true;
                                }
                                configDriversFiles.Add(driversToCopyForThisHost[k].Substring(driversToCopyForThisHost[k].LastIndexOf("\\") + 1), driversToCopyForThisHost[k]);
                            }
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i+1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nКопирование prnport.vbs", i+1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(1);
                            try
                            {
                                File.Copy(Directory.GetCurrentDirectory() + @"\Drivers\prnport.vbs", @"\\" + arrRemPC[i] + @"\C$\Windows\Temp\Drivers\prnport.vbs", true);
                            }
                            catch (Exception e)
                            {
                                errorLog += e.ToString() + "\r\n";
                                err = true;
                            }
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i+1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nКопирование prncnfg.vbs", i+1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(1);
                            try
                            {
                                File.Copy(Directory.GetCurrentDirectory() + @"\Drivers\prncnfg.vbs", @"\\" + arrRemPC[i] + @"\C$\Windows\Temp\Drivers\prncnfg.vbs", true);
                            }
                            catch (Exception e)
                            {
                                errorLog += e.ToString() + "\r\n";
                                err = true;
                            }
                            if (err)
                            {
                                // Если при копировании драйверов произошла ошибка, перейти к следующему удаленному компьютеру.
                                continue;
                            }
                            //-----------------------------------------------------------------------------------------
                            #endregion
                            #region Подключение принтеров
                            //-----------------------------------------------------------------------------------------
                            // Подключение принтеров
                            #region Создание .bat файла на удаленном компьютере
                            // Создание .bat файла на удаленном компьютере
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i + 1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nФормирование пакетного файла...", i + 1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(250);
                            string sBatFile = @"\\" + arrRemPC[i] + @"\C$\Windows\Temp\Drivers\setupprn.bat";
                            try
                            {
                                if (File.Exists(sBatFile))
                                {
                                    File.Delete(sBatFile);
                                }
                                StreamWriter sw = new StreamWriter(sBatFile, false, Encoding.GetEncoding("cp866"));
                                sw.WriteLine(@"@echo off");
                                for (int l = 0; l < printersForInstall.Count; l++)
                                {
                                    Dictionary<string, string> configData = GetConfigData(configDriversFiles[printersForInstall[l].Name] + "\\configRSP.cfg", ref errorLog);
                                    if (configData == null) return;
                                    sw.WriteLine("echo #connect");
                                    sw.WriteLine("echo Подключение принтера " + configData["printerName"].TrimEnd('\r', '\n') + "__" + printersForInstall[l].IPAdress + " на ПК " + arrRemPC[i]);
                                    sw.WriteLine("echo =Создание порта для подключения принтера " + printersForInstall[l].IPAdress);
                                    string command = @"C:\windows\system32\cscript.exe C:\Windows\Temp\Drivers\prnport.vbs -a -r IP_" + printersForInstall[l].IPAdress + @" -h " + printersForInstall[l].IPAdress + @" -o raw -n 9100";
                                    sw.WriteLine(command);
                                    sw.WriteLine("echo Завершено.\necho.");
                                    sw.WriteLine("echo Подключение принтера " + printersForInstall[l].IPAdress);
                                    command = @"c:\windows\system32\rundll32.exe printui.dll,PrintUIEntry /q /if /b " + configData["printerName"].TrimEnd('\r', '\n') +"_" + printersForInstall[l].IPAdress + @" /f " + quote + @"C:\Windows\Temp\Drivers\" + printersForInstall[l].Name + "\\" + printersForInstall[l].Name.Replace(" ", "") + ".inf" + quote + @" /r IP_" + printersForInstall[l].IPAdress + @" /m " + quote + configData["nameModelDriver"].TrimEnd('\r', '\n') + quote + @" /z";
                                    sw.WriteLine(command);
                                    sw.WriteLine("echo Завершено.\necho.");
                                    sw.WriteLine("echo Проверка подключения принтера " + printersForInstall[l].IPAdress);
                                    command = @"C:\windows\system32\cscript.exe C:\Windows\Temp\Drivers\prncnfg.vbs -g -p " + quote + configData["printerName"].TrimEnd('\r', '\n') +"_" + printersForInstall[l].IPAdress + quote;
                                    sw.WriteLine(command);
                                    sw.WriteLine("echo Завершено.\n");
                                }
                                sw.Close();
                            }
                            catch (Exception ex)
                            {
                                errorLog += "Ошибка при создании .bat файла на удаденном компьютере " + arrRemPC[i] + ":\r\n" + ex.Message;
                                err = true;
                            }
                            if (err)
                            {
                                // Если при создании .bat файла произошла ошибка, перейти к следующему удаленному компьютеру.
                                continue;
                            }
                            #endregion
                            #region Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                            //-----------------------------------------------------------------------------------------
                            // Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i + 1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nПодключение принтеров...", i + 1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(250);
                            bool procIsRuned = false;
                            string runCommand = @"cmd.exe /c C:\Windows\Temp\Drivers\setupprn.bat > C:\Windows\Temp\Drivers\logruned.log 2>&1";
                            int?[] res = RunProcessOnRemoteHost(arrRemPC[i], runCommand);
                            if (res[1] != null)
                            {
                                //остановка дальнейшей обработки до тех пор пока существует процесс с указанным ID
                                procIsRuned = true;
                                try
                                {
                                    ManagementScope scope = new ManagementScope("\\\\" + arrRemPC[i] + "\\root\\cimv2");
                                    scope.Connect();
                                    ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process WHERE ProcessID = " + res[1]);
                                    while (procIsRuned)
                                    {
                                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                                        if (searcher.Get().Count <= 0)
                                        {
                                            procIsRuned = false;
                                        }
                                        Thread.Sleep(100);
                                    }
                                }
                                catch (ManagementException e)
                                {
                                   errorLog += "Произошла ошибка во время запроса данных WMI с удаленного компьютера " + arrRemPC[i] + ":\r\n" + e.Message;
                                    err = true;
                                }
                                catch (System.UnauthorizedAccessException unauthorizedErr)
                                {
                                    errorLog += "Ошибка соединения с удаленным компьютером " + arrRemPC[i] + ":\r\n" + unauthorizedErr.Message;
                                    err = true;
                                }
                                if (err)
                                {
                                    // Если в процессе подключения принтеров произошла ошибка, перейти к следующему удаленному компьютеру.
                                    continue;
                                }
                                // по окончанию выполнения процесса считать лог работы.
                                if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i + 1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nЧтение лог файла...", i + 1, arrRemPC[i], arrRemPC.Length))
                                    return;
                                Thread.Sleep(250);
                                try
                                {
                                    using (StreamReader sr = new StreamReader(@"\\" + arrRemPC[i] + @"\C$\Windows\Temp\Drivers\logruned.log", Encoding.GetEncoding("cp866")))
                                    {
                                        logRunedProcess += sr.ReadToEnd();
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorLog += "Не удалось прочитать файл лога с удаленного компьютера " + arrRemPC[i] + ":\r\n" + e.Message + "\r\n";
                                    err = true;
                                }
                            }
                            else
                            {
                                err = true;
                                switch (res[0])
                                {
                                    case 2:
                                        errorLog += "Не удалось запустить процесс на удаленном компьютере " + arrRemPC[i] + ", отказанно в доступе!!";
                                        break;
                                    case 3:
                                        errorLog += "Не удалось запустить процесс на удаленном компьютере " + arrRemPC[i] + ", недостаточно прав!!";
                                        break;
                                    case 8:
                                        errorLog += "Не удалось запустить процесс на удаленном компьютере " + arrRemPC[i] + ", неизвестная ошибка!!";
                                        break;
                                    case 9:
                                        errorLog += "Не удалось запустить процесс на удаленном компьютере " + arrRemPC[i] + ", путь не найден!!";
                                        break;
                                    case 21:
                                        errorLog += "Не удалось запустить процесс на удаленном компьютере " + arrRemPC[i] + ", ошибочные параметры!!";
                                        break;
                                    default:
                                        errorLog += "Не удалось запустить процесс на удаленном компьютере " + arrRemPC[i];
                                        break;
                                }
                            }
                            //-----------------------------------------------------------------------------------------
                            #endregion
                            //-----------------------------------------------------------------------------------------
                            #endregion
                            #region Удаление скопированных файлов
                            if (ProgressDialog.ProgressDialog.ReportWithCancellationCheck(bw, we, (i + 1) * 100 / arrRemPC.Length, "Обработка {1} {0}/{2}...\nУдаление скопированных файлов...", i + 1, arrRemPC[i], arrRemPC.Length))
                                return;
                            Thread.Sleep(1);
                            try
                            {
                                Directory.Delete(@"\\" + arrRemPC[i] + @"\C$\Windows\Temp\Drivers", true);
                            }
                            catch (Exception e)
                            {
                                errorLog += "Ошибка удаления папки на удаленном компьютере: " + e.Message + "\r\n";
                            }
                            if (err)
                            {
                                // Если при удалении скопированных файлов произошла ошибка, перейти к следующему удаленному компьютеру.
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            offlinePC += arrRemPC[i] + ", ";
                        }
                        //-----------------------------------------------------------------------------------------
                        #endregion
                    }
                }
                //Конец обработки списка удаленных компьютеров----------------------------------------------------------
                #endregion

                ProgressDialog.ProgressDialog.CheckForPendingCancellation(bw, we);
            }, new ProgressDialogSettings(true, true, false));

            if (result.Cancelled){
                ReportWindow.ReportWindow _rW = new ReportWindow.ReportWindow("error\r\n" + errorLog + "\r\n" + "#offline\r\n" + offlinePC + "\r\n" + logRunedProcess);
                _rW.ShowDialog();
            }else if (result.OperationFailed){
                MessageBox.Show("Ошибка!!!.");
            }else{
                ReportWindow.ReportWindow _rW = new ReportWindow.ReportWindow("error\r\n" + errorLog + "\r\n" + "#offline\r\n" + offlinePC + "\r\n" + logRunedProcess);
                _rW.ShowDialog();
            }
        }
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void DelPrintersOnRemPC()
        {
            #region Проверка доступности удаленного компьютера, изменений поля с именем хоста и выбранных принтеров для удаления
            if (!string.IsNullOrWhiteSpace(NameHosts))
            {
                // Проверка изменений в поле удаленного компьютера
                if (PrevNameHosts != NameHosts)
                {
                    MessageBox.Show("Наименование удаленного компьютера было изменено.\nПовторите сканирование!");
                    return;
                }
                // Проверка доступности удаленного компьютера перед подключением
                if (!PingHost(NameHosts))
                {
                    MessageBox.Show("Удаленный компьютер не доступен!!");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Не указан удаленный компьютер!!");
                return;
            }
            if (_installedPrinters.SelectedPrinterCount() <= 0)
            {
                MessageBox.Show("Не выбраны принтеры для удаления!!");
                return;
            }
            #endregion
            const string quote = "\"";
            bool error = false;
            string errorMessage = "Ошибка удаления принтера:\r\n";
            List<string> regPrnToDel = new List<string>();
            List<string> namePrnToDel = new List<string>();
            #region Запускаем новый поток
            // Запускаем новый поток
            Task.Factory.StartNew(() =>
            {
                // Усыпляем ненадолго поток
                Thread.Sleep(10);
                #region Выводим сообщение в строку состояния и блокируем интерфейс
                // Вызываем изменение текста лейбла в потоке UI
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = false);
                #endregion

                #region Создание .bat файла на удаленном компьютере
                // Создание .bat файла на удаленном компьютере
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Формирование пакетного файла...");
                Thread.Sleep(250);
                string sBatFile = @"\\" + NameHosts + @"\C$\Windows\Temp\delprn.bat";
                int printersToDelCount = 0;
                try
                {
                    if (File.Exists(sBatFile))
                    {
                        File.Delete(sBatFile);
                    }
                    StreamWriter sw = new StreamWriter(sBatFile, false, Encoding.GetEncoding("cp866"));
                    sw.WriteLine(@"@echo off");
                    sw.WriteLine("net stop spooler");
                    sw.WriteLine(@"del /f /q %systemroot%\system32\spool\printers\*.shd");
                    sw.WriteLine(@"del /f /q %systemroot%\system32\spool\printers\*.spl");
                    for (int i = 0; i < InstalledPrintersOnRemPC.Count; i++)
                    {
                        if (InstalledPrintersOnRemPC[i].IsSelected)
                        {
                            printersToDelCount++;
                            if (!string.IsNullOrWhiteSpace(InstalledPrintersOnRemPC[i].RegKey))
                                regPrnToDel.Add(InstalledPrintersOnRemPC[i].RegKey);
                            else
                                namePrnToDel.Add(InstalledPrintersOnRemPC[i].Name);
                        }
                    }
                    if (regPrnToDel.Count > 0)
                    {
                        regPrnToDel.ForEach(x => { sw.WriteLine(@"REG DELETE " + x + " /f"); });
                        sw.WriteLine("net start spooler");
                    }
                    if (namePrnToDel.Count > 0)
                    {
                        if (regPrnToDel.Count <= 0) sw.WriteLine("net start spooler");
                        namePrnToDel.ForEach(o => { sw.WriteLine(@"rundll32.exe printui.dll,PrintUIEntry /dl /n " + quote + o + quote + @" /q"); });
                    }
                    sw.Close();
                }
                catch (Exception ex)
                {
                    errorMessage += "Ошибка при создании .bat файла на удаденном компьютере " + NameHosts + ":\r\n" + ex.Message;
                    error = true;
                }
                #endregion
                // Если при создании .bat файла ошибок нет, запускаем выполнение созданного .bat файла на удаленном компьютере.
                if (!error)
                {
                    #region Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                    //-----------------------------------------------------------------------------------------
                    // Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                    if (printersToDelCount <= 1)
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Удаление принтера " + InstalledPrintersOnRemPCItem.Name);
                    else
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Удаление выбранных принтеров...");
                    bool procIsRuned = false;
                    string runCommand = @"cmd.exe /c C:\Windows\Temp\delprn.bat";
                    int?[] res = RunProcessOnRemoteHost(NameHosts, runCommand);
                    if (res[1] != null)
                    {
                        //остановка дальнейшей обработки до тех пор пока существует процесс с указанным ID
                        procIsRuned = true;
                        try
                        {
                            ManagementScope scope = new ManagementScope("\\\\" + NameHosts + "\\root\\cimv2");
                            scope.Connect();
                            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process WHERE ProcessID = " + res[1]);
                            while (procIsRuned)
                            {
                                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                                if (searcher.Get().Count <= 0)
                                {
                                    procIsRuned = false;
                                }
                                Thread.Sleep(100);
                            }
                        }
                        catch (ManagementException e)
                        {
                            errorMessage += "Произошла ошибка во время запроса данных WMI с удаленного компьютера " + NameHosts + ":\r\n" + e.Message;
                            error = true;
                        }
                        catch (System.UnauthorizedAccessException unauthorizedErr)
                        {
                            errorMessage += "Ошибка соединения с удаленным компьютером " + NameHosts + ":\r\n" + unauthorizedErr.Message;
                            error = true;
                        }
                    }
                    else
                    {
                        error = true;
                        switch (res[0])
                        {
                            case 2:
                                errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", отказанно в доступе!!";
                                break;
                            case 3:
                                errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", недостаточно прав!!";
                                break;
                            case 8:
                                errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", неизвестная ошибка!!";
                                break;
                            case 9:
                                errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", путь не найден!!";
                                break;
                            case 21:
                                errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", ошибочные параметры!!";
                                break;
                            default:
                                errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts;
                                break;
                        }
                    }
                    //-----------------------------------------------------------------------------------------
                    #endregion
                }
                // Если в процессе выполнения .bat файла ошибок не возникло, обновляем список установленных принтеров
                if (!error)
                {
                    #region Обновление списка установленных принтеров
                    DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Обновление списка установленных принтеров...");
                    string errStr = "";
                    _installedPrintersOnRemPCWithUsers = null;
                    DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrintersOnRemPCWithUsers = GetInstalledPrintersOnRemotePC(NameHosts, ref error, ref errStr));
                    while (_installedPrintersOnRemPCWithUsers == null)
                    {
                        Thread.Sleep(500);
                    }
                    if (error)
                    {
                        errorMessage += "Ошибка получения информации об установленных принтерах:\r\n" + errStr;
                    }
                    if (!error)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrinters.updateData(_installedPrintersOnRemPCWithUsers[SelectedUserOnRemPC]));
                    }
                    #endregion
                }
                #region Снимаем блокировку интерфейса и очищаем строку состояния
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = true);
                if (!GetNetFromNameRemPC_isChecked)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = true);
                }
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "");
                DispatcherHelper.CheckBeginInvokeOnUI(() => UpdateSelectionPrintersOnRemPC());
                DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty());
                #endregion
                #region Если были ошибки, выводим сообщение с текстом ошибки
                if (error)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => ShowErrorMessage(errorMessage));
                }
                #endregion
            });
            #endregion
        }
        private void SetDefaultPrintersOnRemPC()
        {
            #region Проверка доступности удаленного компьютера и изменений поля с именем хоста
            if (!string.IsNullOrWhiteSpace(NameHosts))
            {
                // Проверка изменений в поле удаленного компьютера
                if (PrevNameHosts != NameHosts)
                {
                    MessageBox.Show("Наименование удаленного компьютера было изменено.\nПовторите сканирование!");
                    return;
                }
                // Проверка доступности удаленного компьютера перед подключением
                if (!PingHost(NameHosts))
                {
                    MessageBox.Show("Удаленный компьютер не доступен!!");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Не указан удаленный компьютер!!");
                return;
            }
            #endregion
            Dictionary<string, string> Device = new Dictionary<string, string>();
            string errorMsg = "";
            bool error = false;
            RegistryKey printKeyU;
            bool isFind = false;

            #region Запускаем новый поток
            // Запускаем новый поток
            Task.Factory.StartNew(() =>
            {
                // Усыпляем ненадолго поток
                Thread.Sleep(10);
                #region Выводим сообщение в строку состояния и блокируем интерфейс
                // Вызываем изменение текста лейбла в потоке UI
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Установка принтера по умолчанию " + InstalledPrintersOnRemPCItem.Name);
                #endregion

                try
                {
                    printKeyU = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, NameHosts);
                    String[] namesU = printKeyU.GetSubKeyNames();
                    foreach (String s in namesU)
                    {
                        String[] key = printKeyU.OpenSubKey(s).GetSubKeyNames();
                        foreach (String k in key)
                        {
                            if (k == "Volatile Environment")
                            {
                                //Получение логина пользователя
                                RegistryKey CurUserKey = printKeyU.OpenSubKey(s + "\\" + k);
                                string user = "";
                                if (CurUserKey != null)
                                {
                                    var name = CurUserKey.GetValue("USERNAME");
                                    if (name != null)
                                    {
                                        user = (string)name;
                                    }
                                }
                                else
                                {
                                    user = "Unknow";
                                }
                                if (SelectedUserOnRemPC == user)
                                {
                                    isFind = true;
                                    #region Получение списка принтеров доступных для установки по умолчанию.
                                    RegistryKey deviceKey = printKeyU.OpenSubKey(s + @"\Software\Microsoft\Windows NT\CurrentVersion\Devices");
                                    if (deviceKey != null)
                                    {
                                        foreach (string valueNames in deviceKey.GetValueNames())
                                        {
                                            Device.Add(valueNames, valueNames + "," + deviceKey.GetValue(valueNames).ToString());
                                        }
                                    }
                                    else
                                    {
                                        errorMsg += "Не удалось получить список принтеров доступных для установки по умолчанию!!!";
                                        error = true;
                                    }
                                    #endregion
                                    // если при получении списка принтеров ошибок не возникло меняем принтер по умолчанию
                                    if (!error)
                                    {
                                        #region Изменение принтера поумолчанию
                                        RegistryKey defaultsKey = printKeyU.OpenSubKey(s + @"\Printers\Defaults");
                                        if (defaultsKey != null)
                                        {
                                            printKeyU.DeleteSubKeyTree(s + @"\Printers\Defaults");
                                            using (RegistryKey defKey = printKeyU.CreateSubKey(s + @"\Printers\Defaults"))
                                            {
                                                defKey.SetValue("Disabled", 1);
                                            }
                                        }
                                        RegistryKey defPrintKey = printKeyU.OpenSubKey(s + @"\Software\Microsoft\Windows NT\CurrentVersion\Windows", RegistryKeyPermissionCheck.ReadWriteSubTree);
                                        if (defPrintKey != null)
                                        {
                                            defPrintKey.SetValue("Device", Device[InstalledPrintersOnRemPCItem.Name], RegistryValueKind.String);
                                            DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrinters.setDefaultPrinter(InstalledPrintersOnRemPCItem.Name));
                                            DispatcherHelper.CheckBeginInvokeOnUI(() => 
                                                {
                                                    _installedPrintersOnRemPCWithUsers[SelectedUserOnRemPC].ToList().ForEach(x =>
                                                    {
                                                        if (x.Name == InstalledPrintersOnRemPCItem.Name)
                                                            x.IsDefault = new BitmapImage(new Uri(@"/RemoteSetupPrinters;component/Resources/defaultPrinter.ico", UriKind.RelativeOrAbsolute));
                                                        else
                                                            x.IsDefault = null;
                                                    });
                                                });
                                        }
                                        else
                                        {
                                            errorMsg += "Не удалось установить принтер по умолчанию:\r\nНе найден ключ реестра!!!";
                                            error = true;
                                        }
                                        #endregion
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    errorMsg += "error: " + e.ToString();
                    error = true;
                }

                #region Снимаем блокировку интерфейса и очищаем строку состояния
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = true);
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = true);
                if (!GetNetFromNameRemPC_isChecked)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = true);
                }
                DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "");
                DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty());
                #endregion
                #region Если были ошибки, выводим сообщение с текстом ошибки
                if (error)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => ShowErrorMessage(errorMsg));
                }
                #endregion
                #region Если выбранный пользователь не найден в реестре необходимо пересканировать
                if (!isFind)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => MessageBox.Show("Не найден выбранный пользователь!!\r\nВыполните повторное сканирование удаленного компьютера.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning));
                }
                #endregion
            });
            #endregion
        }
        private void PrintTestPagePrintersOnRemPC()
        {
            #region Проверка доступности удаленного компьютера, изменений поля с именем хоста и выбранных принтеров для удаления
            if (!string.IsNullOrWhiteSpace(NameHosts))
            {
                // Проверка изменений в поле удаленного компьютера
                if (PrevNameHosts != NameHosts)
                {
                    MessageBox.Show("Наименование удаленного компьютера было изменено.\nПовторите сканирование!");
                    return;
                }
                // Проверка доступности удаленного компьютера перед подключением
                if (!PingHost(NameHosts))
                {
                    MessageBox.Show("Удаленный компьютер не доступен!!");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Не указан удаленный компьютер!!");
                return;
            }
            if (_installedPrinters.SelectedPrinterCount() <= 0)
            {
                MessageBox.Show("Не выбраны принтеры для удаления!!");
                return;
            }
            #endregion
            const string quote = "\"";
            bool error = false;
            string errorMessage = "Ошибка печати тестовой страници:\r\n";
            List<string> regPrnToDel = new List<string>();
            List<string> namePrnToDel = new List<string>();
            if (string.IsNullOrWhiteSpace(InstalledPrintersOnRemPCItem.RegKey))
            {
                #region Запускаем новый поток
                // Запускаем новый поток
                Task.Factory.StartNew(() =>
                {
                    // Усыпляем ненадолго поток
                    Thread.Sleep(10);
                    #region Выводим сообщение в строку состояния и блокируем интерфейс
                    // Вызываем изменение текста лейбла в потоке UI
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = false);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = false);
                    #endregion

                    #region Создание .bat файла на удаленном компьютере
                    // Создание .bat файла на удаленном компьютере
                    DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Формирование пакетного файла...");
                    Thread.Sleep(250);
                    string sBatFile = @"\\" + NameHosts + @"\C$\Windows\Temp\printDefaultPage.bat";
                    try
                    {
                        if (File.Exists(sBatFile))
                        {
                            File.Delete(sBatFile);
                        }
                        StreamWriter sw = new StreamWriter(sBatFile, false, Encoding.GetEncoding("cp866"));
                        sw.WriteLine(@"@echo off");
                        sw.WriteLine(@"rundll32.exe printui.dll,PrintUIEntry /k /n " + quote + InstalledPrintersOnRemPCItem.Name + quote + @" /q");
                        sw.Close();
                    }
                    catch (Exception ex)
                    {
                        errorMessage += "Ошибка при создании .bat файла на удаденном компьютере " + NameHosts + ":\r\n" + ex.Message;
                        error = true;
                    }
                    #endregion
                    // Если при создании .bat файла ошибок нет, запускаем выполнение созданного .bat файла на удаленном компьютере.
                    if (!error)
                    {
                        #region Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                        //-----------------------------------------------------------------------------------------
                        // Формирование строки запуска bat файла и запуск процесса на удаленном компьютере
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "Печать тестовой страници на принтере " + InstalledPrintersOnRemPCItem.Name);
                        bool procIsRuned = false;
                        string runCommand = @"cmd.exe /c C:\Windows\Temp\printDefaultPage.bat";
                        int?[] res = RunProcessOnRemoteHost(NameHosts, runCommand);
                        if (res[1] != null)
                        {
                            //остановка дальнейшей обработки до тех пор пока существует процесс с указанным ID
                            procIsRuned = true;
                            try
                            {
                                ManagementScope scope = new ManagementScope("\\\\" + NameHosts + "\\root\\cimv2");
                                scope.Connect();
                                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process WHERE ProcessID = " + res[1]);
                                while (procIsRuned)
                                {
                                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                                    if (searcher.Get().Count <= 0)
                                    {
                                        procIsRuned = false;
                                    }
                                    Thread.Sleep(100);
                                }
                            }
                            catch (ManagementException e)
                            {
                                errorMessage += "Произошла ошибка во время запроса данных WMI с удаленного компьютера " + NameHosts + ":\r\n" + e.Message;
                                error = true;
                            }
                            catch (System.UnauthorizedAccessException unauthorizedErr)
                            {
                                errorMessage += "Ошибка соединения с удаленным компьютером " + NameHosts + ":\r\n" + unauthorizedErr.Message;
                                error = true;
                            }
                        }
                        else
                        {
                            error = true;
                            switch (res[0])
                            {
                                case 2:
                                    errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", отказанно в доступе!!";
                                    break;
                                case 3:
                                    errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", недостаточно прав!!";
                                    break;
                                case 8:
                                    errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", неизвестная ошибка!!";
                                    break;
                                case 9:
                                    errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", путь не найден!!";
                                    break;
                                case 21:
                                    errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts + ", ошибочные параметры!!";
                                    break;
                                default:
                                    errorMessage += "Не удалось запустить процесс на удаленном компьютере " + NameHosts;
                                    break;
                            }
                        }
                        //-----------------------------------------------------------------------------------------
                        #endregion
                    }

                    #region Снимаем блокировку интерфейса и очищаем строку состояния
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = true);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = true);
                    if (!GetNetFromNameRemPC_isChecked)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = true);
                    }
                    DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "");
                    DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty());
                    #endregion
                    #region Если были ошибки, выводим сообщение с текстом ошибки, иначе выводим сообщение об удачной отправке на печать
                    if (error)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => ShowErrorMessage(errorMessage));
                    }
                    else
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => MessageBox.Show("Тестовая страница успешно отправлена на принтер " + InstalledPrintersOnRemPCItem.Name,"Информация",MessageBoxButton.OK,MessageBoxImage.Information));
                    }
                    #endregion
                });
                #endregion
            }
            else
            {
                MessageBox.Show("К сожалению печать тестовой страници для данного принтера возможна только локально.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ChangeUserOnRemPC()
        {
            #region Проверка изменений поля с именем хоста
            // Проверка изменений в поле удаленного компьютера
            if (PrevNameHosts != NameHosts)
            {
                MessageBox.Show("Наименование удаленного компьютера было изменено.\nПовторите сканирование!");
                return;
            }
            #endregion
            #region Запускаем новый поток
                // Запускаем новый поток
                Task.Factory.StartNew(() =>
                {
                    // Усыпляем ненадолго поток
                    Thread.Sleep(10);
                    if (!_scanIsRuned)
                    {
                        #region Выводим сообщение в строку состояния и блокируем интерфейс
                        // Вызываем изменение текста лейбла в потоке UI
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = false);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = false);
                        #endregion
                    }
                    if (!string.IsNullOrWhiteSpace(SelectedUserOnRemPC))
                        DispatcherHelper.CheckBeginInvokeOnUI(() => _installedPrinters.updateData(_installedPrintersOnRemPCWithUsers[SelectedUserOnRemPC]));
                    if (!_scanIsRuned)
                    {
                        #region Снимаем блокировку интерфейса и очищаем строку состояния
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableBtScan = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableOneCompMode = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListCompMode = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableNameHost = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableFirstItem = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableLastItem = true);
                        if (OneCompMode)
                            DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableGetNetFromNameRemPC = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListNetPrinters = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableListPrintersOnRemPC = true);
                        DispatcherHelper.CheckBeginInvokeOnUI(() => IsEnableCurUserListOnRemPC = true);
                        if (!GetNetFromNameRemPC_isChecked)
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() => NetworkIsEnable = true);
                        }
                        DispatcherHelper.CheckBeginInvokeOnUI(() => StatusText = "");
                        DispatcherHelper.CheckBeginInvokeOnUI(() => UpdateSelectionPrintersOnRemPC());
                        DispatcherHelper.CheckBeginInvokeOnUI(() => PrintersOnRemPCButtonIsEnable = PrintersOnRemPCViewNotEmpty());
                        #endregion
                    }
                });
            #endregion
        }
        #endregion

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}