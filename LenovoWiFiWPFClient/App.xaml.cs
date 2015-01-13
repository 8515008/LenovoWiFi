using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        // {23ED1551-904E-4874-BA46-DBE1489D4D34}
        public static Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");

        private Thread _pipeServerThread = new Thread(ListeningPipeServer);
        public HostedNetworkServiceClient Client { get; private set; }

        private static void ListeningPipeServer()
        {
            using (var pipe = new NamedPipeServerStream(@"\\.\pipe\LenovoWiFi", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
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
                        bool exit = false;
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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _pipeServerThread.Start();

            //var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();

            //if (!trayDeskBand.IsDeskBandShown(CLSIDLenovoWiFiDeskBand))
            //{
            //    trayDeskBand.ShowDeskBand(CLSIDLenovoWiFiDeskBand);
            //}

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
