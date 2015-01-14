using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Threading;
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
        private readonly BackgroundWorker _pipeServerWorker = new BackgroundWorker();
        private readonly Semaphore _workerSemaphore = new Semaphore(0, 1);

        public HostedNetworkServiceClient Client { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            _pipeServerWorker.DoWork += (sender, args) => ListeningPipe();
            _pipeServerWorker.RunWorkerAsync();

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
            });

            startupTask.Start();
        }

        private void ListeningPipe()
        {
            using (var pipe = new NamedPipeServerStream(@"\\.\pipe\LenovoWiFi", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                ShowDeskband();

                var reader = new StreamReader(pipe);
                var writer = new StreamWriter(pipe);

                try
                {
                    pipe.WaitForConnection();

                    writer.WriteLine("connected");
                    writer.Flush();
                    pipe.WaitForPipeDrain();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var exit = false;
                        switch (line)
                        {
                            case "mouseenter":
                                break;
                            case "mouseleave":
                                break;
                            case "lbuttonclick":
                                break;
                            case "exit":
                                exit = true;
                                break;
                        }

                        if (exit)
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    pipe.WaitForPipeDrain();
                    if (pipe.IsConnected)
                    {
                        pipe.Disconnect();
                    }
                }
            }

            _workerSemaphore.Release();
        }

        private void ShowDeskband()
        {
            var clsidLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");

            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();

            if (!trayDeskBand.IsDeskBandShown(clsidLenovoWiFiDeskBand))
            {
                trayDeskBand.ShowDeskBand(clsidLenovoWiFiDeskBand);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _workerSemaphore.WaitOne();
            base.OnExit(e);
        }
    }
}
