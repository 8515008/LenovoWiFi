using System.ServiceModel;
using System.Windows;

namespace Lenovo.WiFi.Client.Model
{
    public class HostedNetwork : IHotspot, IHostedNetworkServiceCallback
    {
        private HostedNetworkClient _client;

        public HostedNetwork()
        {
            this.IsStarted = false;
        }

        private HostedNetworkClient ServiceClient
        {
            get { return _client ?? (_client = new HostedNetworkClient(new InstanceContext(Application.Current))); }
        }

        public string SSID
        {
            get { return this.ServiceClient.GetHostedNetworkName(); }
            set { this.ServiceClient.SetHostedNetworkName(value); }
        }

        public string PresharedKey
        {
            get { return this.ServiceClient.GetHostedNetworkKey(); }
            set { this.ServiceClient.SetHostedNetworkKey(value); }
        }

        public string Authentication { get { return this.ServiceClient.GetHostedNetworkAuthAlgorithm(); } }

        public bool IsStarted { get; set; }

        public int ConnectedDeviceCount { get { return this.ServiceClient.GetHostedNetworkConnectedDeviceCount(); } }

        public void Start()
        {
            this.ServiceClient.StartHostedNetwork();
            this.ServiceClient.RegisterForNewConnectedDevice();

            this.IsStarted = true;
        }

        public void Stop()
        {
            this.ServiceClient.UnregisterForNewConnectedDevice();
            this.ServiceClient.StopHostedNetwork();

            this.IsStarted = false;
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void DeviceConnected(byte[] macAddress)
        {
            throw new System.NotImplementedException();
        }
    }
}
