using System;
using System.ServiceModel;
using System.ServiceProcess;

namespace Lenovo.WiFi
{
    partial class WindowsService : ServiceBase
    {
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

            _serviceHost = new ServiceHost(typeof(HostedNetworkService));
            _serviceHost.Open();
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
