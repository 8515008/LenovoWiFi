using System.ServiceModel;
using Lenovo.WiFi;

namespace LenovoWiFiClient
{
    internal class HostedNetworkProxy
    {
        readonly ChannelFactory<IHostedNetworkService> _pipeFactory =
        new ChannelFactory<IHostedNetworkService>(
          new NetNamedPipeBinding(),
          new EndpointAddress(
            "net.pipe://localhost/LenovoWiFiService/HostedNetworkService/Pipe"));

        internal IHostedNetworkService Proxy
        {
            get { return _pipeFactory.CreateChannel(); }
        }
    }
}
