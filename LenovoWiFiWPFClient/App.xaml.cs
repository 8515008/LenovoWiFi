using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Lenovo.WiFi.Client.Proxy;
using Lenovo.WiFi.Client.Windows;

namespace Lenovo.WiFi.Client
{
    public partial class App : Application
    {
        private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");

        private readonly BackgroundWorker _pipeServerWorker = new BackgroundWorker();

        private Window _currentWindow;

        public HostedNetworkServiceClient Client { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShowDeskband();

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            this.Client = new HostedNetworkServiceClient();

            _pipeServerWorker.DoWork += (sender, args) => ListeningPipe();
            _pipeServerWorker.RunWorkerAsync();

            //var startupWindow = new StartupWindow();
            //startupWindow.Show();

            //var startupTask = new Task(() =>
            //{
            //    this.Client.StartHostedNetwork();
            //});

            //startupTask.ContinueWith(task =>
            //{
            //    var successWindow = new SuccessWindow();
            //    successWindow.Loaded += (sender, args) => startupWindow.Close();
            //    successWindow.Closed += (sender, args) => this.Client.StopHostedNetwork();
            //    this.MainWindow = successWindow;
            //    this.MainWindow.Show();
            //}, TaskScheduler.FromCurrentSynchronizationContext());

            //startupTask.Start();
        }

        private void ListeningPipe()
        {
            using (var pipe = new NamedPipeServerStream("LenovoWiFi", PipeDirection.In, 1, PipeTransmissionMode.Message))
            {
                // ShowDeskband();

                var reader = new StreamReader(pipe, Encoding.Unicode);
                // var writer = new StreamWriter(pipe);

                try
                {
                    pipe.WaitForConnection();

                    string line;
                    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                    {
                        var exit = false;
                        switch (line)
                        {
                            case "mouseenter":
                                this.Dispatcher.BeginInvoke(new Action(ShowStatusWindow));
                                break;
                            case "mouseleave":
                                this.Dispatcher.BeginInvoke(new Action(CloseCurrentWindow));
                                break;
                            case "lbuttonclick":
                                this.Dispatcher.BeginInvoke(new Action(ShowMainWindow));
                                break;
                            case "rbuttonclick":
                                this.Dispatcher.BeginInvoke(new Action(CloseCurrentWindow));
                                break;
                            case "exit":
                                this.Dispatcher.BeginInvoke(new Action(HideDeskband));

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
                    if (pipe.IsConnected)
                    {
                        pipe.Disconnect();
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action(Shutdown));
        }

        private void CloseCurrentWindow()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
            }
        }

        private void ShowStatusWindow()
        {
            CloseCurrentWindow();

            _currentWindow = new StatusWindow();
            _currentWindow.Show();
        }

        private void ShowMainWindow()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
            }

            _currentWindow = new MainWindow();
            _currentWindow.Show();
        }

        private void ShowDeskband()
        {
            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();
            trayDeskBand.ShowDeskBand(CLSIDLenovoWiFiDeskBand);
        }

        private void HideDeskband()
        {
            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();
            trayDeskBand.HideDeskBand(CLSIDLenovoWiFiDeskBand);
        }
    }
}
