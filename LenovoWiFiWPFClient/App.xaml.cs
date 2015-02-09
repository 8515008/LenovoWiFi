using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Autofac;
using Lenovo.WiFi.Client.View;
using Lenovo.WiFi.Client.ViewModel;
using IContainer = Autofac.IContainer;
using Lenovo.WiFi.Client.Model;

namespace Lenovo.WiFi.Client
{
    public partial class App : Application, IDisposable, IHostedNetworkServiceCallback, IDeskbandPipeListener
    {
#if DEBUG
        private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");
        //private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{01E04581-4EEE-11d0-BFE9-00AA005B4383}");
#else
        private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");
#endif

        private bool _disposing;
        private readonly IContainer _container = new Bootstrapper().Build();
        private readonly BackgroundWorker _pipeServerWorker = new BackgroundWorker();

        private MainWindow _mainWindow;
        private StatusWindow _statusWindow;

        private Window _currentWindow;
        private bool _mainWindowsShowing;

        private DeskbandWarningDialogHook _dialogHook = new DeskbandWarningDialogHook();
        private static DeskbandPipe pipeWorker = null;

        static public DeskbandPipe DeskBandPipe
        {
            get
            {
                if(null == pipeWorker)
                {
                    pipeWorker = new DeskbandPipe();
                }

                return pipeWorker;
            }
        }

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
            DeskBandPipe.SetDeskbandDelegate(this.OnHandleDeskbandCmd);

            _pipeServerWorker.DoWork += (sender, args) => DeskBandPipe.Start();
            _pipeServerWorker.RunWorkerAsync();

            ShowDeskband();

            var splashWindow = _container.Resolve<SplashWindow>();
            splashWindow.Show();

            Task.Factory.StartNew(() =>
            {
                _container.Resolve<ISplashViewModel>().Start();
            }).ContinueWith(t => 
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashWindow.Close();

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

        //private void ListeningPipe()
        //{
        //    using (var pipe = new NamedPipeServerStream("LenovoWiFi", PipeDirection.In, 1, PipeTransmissionMode.Message))
        //    {
        //        var reader = new StreamReader(pipe, Encoding.Unicode);
        //        // var writer = new StreamWriter(pipe);

        //        while (true)
        //        {
        //            try
        //            {
        //                pipe.WaitForConnection();

        //                var line = reader.ReadLine();
        //                var exit = false;
        //                switch (line)
        //                {
        //                    case "mouseenter":
        //                        this.Dispatcher.BeginInvoke(new Action(OnMouseEnter));
        //                        break;
        //                    case "mouseleave":
        //                        this.Dispatcher.BeginInvoke(new Action(OnMouseLeave));
        //                        break;
        //                    case "lbuttonclick":
        //                        this.Dispatcher.BeginInvoke(new Action(OnLButtonClick));
        //                        break;
        //                    case "rbuttonclick":
        //                        this.Dispatcher.BeginInvoke(new Action(OnRButtonClick));
        //                        break;
        //                    case "exit":
        //                        this.Dispatcher.BeginInvoke(new Action(OnExit));

        //                        exit = true;
        //                        break;
        //                }

        //                if (exit)
        //                {
        //                    break;
        //                }
        //            }
        //            finally
        //            {
        //                if (pipe.IsConnected)
        //                {
        //                    pipe.Disconnect();
        //                }
        //            }
        //        }
        //    }

        //    this.Dispatcher.BeginInvoke(new Action(Shutdown));
        //}

        private int OnHandleDeskbandCmd(DeskbandCommand cmd)
        {
            switch(cmd)
            { 
                case DeskbandCommand.DESKB_mouseenter:
                    this.OnMouseEnter();
                    break;
                case DeskbandCommand.DESKB_mouseleave:
                    this.OnMouseLeave();
                    break;
                case DeskbandCommand.DESKB_lbuttonclick:
                    this.OnLButtonClick();
                    break;
                case DeskbandCommand.DESKB_rbuttonclick:
                    this.OnRButtonClick();
                    break;
                case DeskbandCommand.DESKB_exit:
                    this.OnExit();
                    break;
                default:
                    break;
            }
            return 0;
        }

        public void OnMouseEnter()
        {
            this.Dispatcher.BeginInvoke(new Action(()=> {
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
            }));
        }

        public void OnMouseLeave()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentWindow != null && !(_currentWindow is MainWindow))
                {
                    _currentWindow.Hide();
                    _currentWindow = null;
                }
            }));
        }

        public void OnLButtonClick()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentWindow != null)
                {
                    _currentWindow.Hide();
                }

                _currentWindow = _mainWindow;
                _currentWindow.Show();
            }));
        }

        public void OnRButtonClick()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentWindow != null)
                {
                    _currentWindow.Hide();
                    _currentWindow = null;
                }
            }));
        }

        public void OnExit()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                HideDeskband();
            }));
        }

        private void ShowDeskband()
        {
            _dialogHook.StartHook();

            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();
            trayDeskBand.ShowDeskBand(CLSIDLenovoWiFiDeskBand);

            // TODO: Stop Hooking by receiving pipe message

            Task.Run(() =>
            {
                Thread.Sleep(1000);

                _dialogHook.StopHook();
            });
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
