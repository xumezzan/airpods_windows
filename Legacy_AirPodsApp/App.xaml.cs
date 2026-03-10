using System.Windows;
using System.Threading;

namespace AirPodsApp
{
    public partial class App : Application
    {
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "AirPodsWindows11AppUnique";
            _mutex = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                // App is already running. Exit.
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
