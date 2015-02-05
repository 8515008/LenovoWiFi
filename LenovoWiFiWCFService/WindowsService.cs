using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;

namespace Lenovo.WiFi
{
    partial class WindowsService : ServiceBase
    {
        private const string ServiceLibrary = "LenovoWiFiWCFLibrary.dll";
        private const string ServiceType = "Lenovo.WiFi.HostedNetworkService";

        private ServiceHost _serviceHost;

        public WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
            }

            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

#if !DEBUG
            var latestVersion = new Version(0, 0);
            foreach (var subDirectory in new DirectoryInfo(rootDirectory).GetDirectories())
            {
                var version = new Version(subDirectory.Name);
                if (version > latestVersion)
                {
                    Environment.CurrentDirectory = subDirectory.FullName;
                }
            }

            Task.Run(() =>
            {
                // TODO: CHECK FOR NEW VERSION
                // TODO: DOWNLOAD NEW VERSION IF NECESSARY
            });

            if (Environment.CurrentDirectory == rootDirectory)
            {
                return;
            }
#endif

            var assembly = Assembly.LoadFrom(ServiceLibrary);
            var service = assembly.GetType(ServiceType);


            if (service != null)
            {
                _serviceHost = new ServiceHost(service);
                _serviceHost.Open();
            }
        }

        protected override void OnStop()
        {
            StopService();
        }

        protected override void OnShutdown()
        {
            StopService();
        }

        private void StopService()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
        }

        public static void Main()
        {
            Run(new WindowsService());
        }
    }
}
