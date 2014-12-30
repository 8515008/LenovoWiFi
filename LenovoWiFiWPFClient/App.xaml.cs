using System.Threading.Tasks;
using System.Windows;

using Lenovo.WiFi.Client.Windows;

namespace Lenovo.WiFi.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private HostedNetworkClient _client;

        protected override void OnStartup(StartupEventArgs e)
        {
            var startupWindow = new StartupWindow();
            startupWindow.Show();

            var startupTask = new Task(() =>
            {
                _client.StartHostedNetwork();
            });

            startupTask.ContinueWith(task =>
            {
                var successWindow = new SuccessWindow();
                successWindow.Loaded += (sender, args) => startupWindow.Close();
                this.MainWindow = successWindow;
                this.MainWindow.Show();
            });

            startupTask.Start(TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
