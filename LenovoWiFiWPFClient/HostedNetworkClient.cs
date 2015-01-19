using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Lenovo.WiFi.Client
{
    class HostedNetworkClient : ClientBase<IHostedNetworkService>, IHostedNetworkService
    {
        public HostedNetworkClient() {
        }
        
        public HostedNetworkClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public HostedNetworkClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HostedNetworkClient(string endpointConfigurationName, EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }

        public HostedNetworkClient(Binding binding, EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetHostedNetworkName() {
            return base.Channel.GetHostedNetworkName();
        }
        
        public void SetHostedNetworkName(string name) {
            base.Channel.SetHostedNetworkName(name);
        }
        
        public string GetHostedNetworkKey() {
            return base.Channel.GetHostedNetworkKey();
        }
        
        public void SetHostedNetworkKey(string key) {
            base.Channel.SetHostedNetworkKey(key);
        }
        
        public string GetHostedNetworkAuthAlgorithm() {
            return base.Channel.GetHostedNetworkAuthAlgorithm();
        }
        
        public void StartHostedNetwork() {
            base.Channel.StartHostedNetwork();
        }
        
        public int GetHostedNetworkConnectedDeviceCount() {
            return base.Channel.GetHostedNetworkConnectedDeviceCount();
        }
        
        public void StopHostedNetwork() {
            base.Channel.StopHostedNetwork();
        }
    }
}
