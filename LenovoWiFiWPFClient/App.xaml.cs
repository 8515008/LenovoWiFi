using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Lenovo.WiFi.Client.View;
using Lenovo.WiFi.Client.ViewModel;
using IContainer = Autofac.IContainer;

namespace Lenovo.WiFi.Client
{
    public partial class App : Application, IDisposable, IHostedNetworkServiceCallback
    {
        private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");

        private bool _disposing;
        private readonly IContainer _container = new Bootstrapper().Build();
        private readonly BackgroundWorker _pipeServerWorker = new BackgroundWorker();

        private MainWindow _mainWindow;
        private StatusWindow _statusWindow;

        private Window _currentWindow;
        private bool _mainWindowsShowing;

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
            _pipeServerWorker.DoWork += (sender, args) => ListeningPipe();
            _pipeServerWorker.RunWorkerAsync();

            var splashWindow = _container.Resolve<SplashWindow>();
            splashWindow.Show();

            Task.Factory.StartNew(() =>
            {
                _container.Resolve<ISplashViewModel>().Start();
            }).ContinueWith(t =>
            {
                this.Dispatcher.BeginInvoke(new Action(() => splashWindow.Close()));

                ShowDeskband();

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var successWindow = _container.Resolve<SuccessWindow>();
                    successWindow.DataContext = _container.Resolve<ISuccessViewModel>();
                    successWindow.Show();

                    _currentWindow = successWindow;
                }));
            });

            _mainWindow = _container.Resolve<MainWindow>();
            _mainWindow.DataContext = _container.Resolve<IMainViewModel>();

            _statusWindow = _container.Resolve<StatusWindow>();
            _statusWindow.DataContext = _container.Resolve<IStatusViewModel>();
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
                _currentWindow.Hide();
                _currentWindow = null;
            }

            if (_currentWindow == null)
            {
                _currentWindow = _statusWindow;
                _currentWindow.Show();
            }
        }

        private void OnMouseLeave()
        {
            if (_currentWindow != null && !(_currentWindow is MainWindow))
            {
                _currentWindow.Hide();
                _currentWindow = null;
            }
        }

        private void OnLeftButtonDown()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Hide();
            }

            _currentWindow = _mainWindow;
            _currentWindow.Show();
        }

        private void OnRightButtonDown()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Hide();
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

        public void DeviceConnected(byte[] macAddress)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentWindow != null)
                {
                    _currentWindow.Hide();
                }

                var notification = _container.Resolve<NotificationWindow>();
                notification.DataContext = new NotificationViewModel { Message = BitConverter.ToString(macAddress) };

                notification.Show();
            }));
        }
    }
}
