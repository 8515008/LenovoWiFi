using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Lenovo.WiFi.Client.Windows;

namespace Lenovo.WiFi.Client
{
    public partial class App : Application, IDisposable
    {
        private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");

        private readonly BackgroundWorker _pipeServerWorker = new BackgroundWorker();

        private bool _disposing;
        private Window _currentWindow;
        private bool _mainWindowsShowing;

        internal HostedNetworkClient Client { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pipeServerWorker.Dispose();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            this.Client = new HostedNetworkClient();

            _pipeServerWorker.DoWork += (sender, args) => ListeningPipe();
            _pipeServerWorker.RunWorkerAsync();

            var startupWindow = new StartupWindow();
            startupWindow.Show();

            var startupTask = new Task(() =>
            {
                this.Client.StartHostedNetwork();
            });

            startupTask.ContinueWith(task =>
            {
                ShowDeskband();

                var successWindow = new SuccessWindow();
                successWindow.Loaded += (sender, args) => startupWindow.Close();
                successWindow.Closed += (sender, args) => this.Client.StopHostedNetwork();
                _currentWindow = successWindow;
                this.MainWindow = successWindow;
                this.MainWindow.Show();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupTask.Start();
        }

        private void ListeningPipe()
        {
            using (var pipe = new NamedPipeServerStream("LenovoWiFi", PipeDirection.In, 1, PipeTransmissionMode.Message))
            {
                var reader = new StreamReader(pipe, Encoding.Unicode);
                // var writer = new StreamWriter(pipe);

                while (true)
                {
                    try
                    {
                        pipe.WaitForConnection();

                        var line = reader.ReadLine();
                        var exit = false;
                        switch (line)
                        {
                            case "mouseenter":
                                this.Dispatcher.BeginInvoke(new Action(OnMouseEnter));
                                break;
                            case "mouseleave":
                                this.Dispatcher.BeginInvoke(new Action(OnMouseLeave));
                                break;
                            case "lbuttonclick":
                                this.Dispatcher.BeginInvoke(new Action(OnLeftButtonDown));
                                break;
                            case "rbuttonclick":
                                this.Dispatcher.BeginInvoke(new Action(OnRightButtonDown));
                                break;
                            case "exit":
                                this.Dispatcher.BeginInvoke(new Action(OnExit));

                                exit = true;
                                break;
                        }

                        if (exit)
                        {
                            break;
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
            }

            this.Dispatcher.BeginInvoke(new Action(Shutdown));
        }

        private void OnMouseEnter()
        {
            if (_currentWindow != null && !(_currentWindow is MainWindow))
            {
                _currentWindow.Close();
                _currentWindow = null;
            }

            if (_currentWindow == null)
            {
                _currentWindow = new StatusWindow();
                _currentWindow.Show();
            }
        }

        private void OnMouseLeave()
        {
            if (_currentWindow != null && !(_currentWindow is MainWindow))
            {
                _currentWindow.Close();
                _currentWindow = null;
            }
        }

        private void OnLeftButtonDown()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
            }

            _currentWindow = new MainWindow();
            this.MainWindow = _currentWindow;
            _currentWindow.Show();
        }

        private void OnRightButtonDown()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
                _currentWindow = null;
            }
        }

        private void OnExit()
        {
            HideDeskband();
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
