using System;
using System.ServiceModel;

namespace Lenovo.WiFi.Client
{
    public class HostedNetworkClient
    {
        readonly ChannelFactory<IHostedNetworkService> _pipeFactory =
        new ChannelFactory<IHostedNetworkService>(
          new NetNamedPipeBinding(),
          new EndpointAddress(
            "net.pipe://localhost/LenovoWiFiService/HostedNetworkService/Pipe"));

        IHostedNetworkService _channel;

        private IHostedNetworkService Proxy
        {
            get { return _channel ?? (_channel = _pipeFactory.CreateChannel()); }
        }

        public string GetHostedNetworkName()
        {
            string name;

            this.Proxy.GetHostedNetworkName(out name);

            return name;
        }

        public void SetHostedNetworkName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length < 1 || name.Length > 32)
            {
                throw new ArgumentOutOfRangeException("name");
            }

            this.Proxy.SetHostedNetworkName(name);
        }

        public string GetHostedNetworkKey()
        {
            string key;

            this.Proxy.GetHostedNetworkKey(out key);

            return key;
        }

        public void SetHostedNetworkKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (key.Length < 8 || key.Length > 64)
            {
                throw new ArgumentOutOfRangeException("key");
            }

            this.Proxy.SetHostedNetworkKey(key);
        }

        public void StartHostedNetwork()
        {
            this.Proxy.StartHostedNetwork();
        }

        public void StopHostedNetwork()
        {
            this.Proxy.StopHostedNetwork();
        }
    }
}
