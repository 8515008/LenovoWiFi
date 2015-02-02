using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Lenovo.WiFi
{
    partial class WindowsService : ServiceBase
    {
        private const string ServiceLibrary = "LenovoWiFiWCFLibrary";
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

            var rootDirectory = Environment.CurrentDirectory;

            var latestVersion = new Version(-1, 0);
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

            if (Environment.CurrentDirectory != rootDirectory)
            {
                var assembly = Assembly.LoadFrom(ServiceLibrary);
                var service = assembly.GetType(ServiceType);

                if (service != null)
                {
                    _serviceHost = new ServiceHost(service);
                    _serviceHost.Open();
                }
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
