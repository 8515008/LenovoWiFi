using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;

using NLog;

namespace Lenovo.WiFi
{
    partial class WindowsService : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string ServiceLibrary = "LenovoWiFiWCFLibrary.dll";
        private const string ServiceType = "Lenovo.WiFi.HostedNetworkService";

        private ServiceHost _serviceHost;

        public WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Trace("OnStart: Invoked");

            if (_serviceHost != null)
            {
                Logger.Trace("OnStart: Closing any previous hosted service");
                _serviceHost.Close();
            }

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                Logger.Error("OnStart: Couldn't find the directory where the service resides. Terminating...");
                return;
            }
            
            Environment.CurrentDirectory = Path.GetDirectoryName(assemblyLocation);
            Logger.Info("OnStart: Current directory: {0}", Environment.CurrentDirectory);

#if !DEBUG
            //var latestVersion = new Version(0, 0);
            //foreach (var subDirectory in new DirectoryInfo(rootDirectory).GetDirectories())
            //{
            //    var version = new Version(subDirectory.Name);
            //    if (version > latestVersion)
            //    {
            //        Environment.CurrentDirectory = subDirectory.FullName;
            //    }
            //}

            //Task.Run(() =>
            //{
            //    // TODO: CHECK FOR NEW VERSION
            //    // TODO: DOWNLOAD NEW VERSION IF NECESSARY
            //});

            //if (Environment.CurrentDirectory == rootDirectory)
            //{
            //    return;
            //}
#endif

            try
            {
                var assembly = Assembly.LoadFrom(ServiceLibrary);
                Logger.Trace("OnStart: Service library was found and loaded");
                var service = assembly.GetType(ServiceType);
                Logger.Trace("OnStart: Service was located? {0}", service != null);
            
                if (service != null)
                {
                    _serviceHost = new ServiceHost(service);
                    _serviceHost.Open();
                    Logger.Trace("OnStart: Service is now running...");
                }
            }
            catch (Exception exception)
            {
                Logger.ErrorException("OnStart: Error happened...", exception);
            }
            
        }

        protected override void OnStop()
        {
            Logger.Trace("OnStop: Invoked");
            StopService();
        }

        protected override void OnShutdown()
        {
            Logger.Trace("OnShutdown: Invoked");
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
