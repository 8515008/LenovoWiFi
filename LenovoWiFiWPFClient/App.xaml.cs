using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

using Lenovo.WiFi.Client.Proxy;
using Lenovo.WiFi.Client.Windows;

namespace Lenovo.WiFi.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // {23ED1551-904E-4874-BA46-DBE1489D4D34}
        public static Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");
        public HostedNetworkServiceClient Client { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();
            trayDeskBand.ShowDeskBand(CLSIDLenovoWiFiDeskBand);

            this.Client = new HostedNetworkServiceClient();

            var startupWindow = new StartupWindow();
            startupWindow.Show();

            var startupTask = new Task(() =>
            {
                this.Client.StartHostedNetwork();
            });

            startupTask.ContinueWith(task =>
            {
                var successWindow = new SuccessWindow();
                successWindow.Loaded += (sender, args) => startupWindow.Close();
                successWindow.Closed += (sender, args) => this.Client.StopHostedNetwork();
                this.MainWindow = successWindow;
                this.MainWindow.Show();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupTask.Start();
        }
    }
}
