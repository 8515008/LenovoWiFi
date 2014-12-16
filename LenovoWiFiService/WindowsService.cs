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

            _serviceHost = new ServiceHost(typeof(DaemonService));
            _serviceHost.Open();
        }

        protected override void OnStop()
        {
            CloseDaemonService();
        }

        protected override void OnShutdown()
        {
            CloseDaemonService();
        }

        private void CloseDaemonService()
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
