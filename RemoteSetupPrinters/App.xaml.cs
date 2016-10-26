using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System;

namespace RemoteSetupPrinters
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static String[] mArgs;

        static App()
        {
            DispatcherHelper.Initialize();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                mArgs = e.Args;
            }
        }
    }
}
