using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Lenovo.WiFi.Client
{
    public class HostedNetworkClient : DuplexClientBase<IHostedNetworkService>, IHostedNetworkService
    {

        public HostedNetworkClient(InstanceContext callbackInstance) :
            base(callbackInstance)
        {
        }

        public HostedNetworkClient(InstanceContext callbackInstance, string endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public HostedNetworkClient(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public HostedNetworkClient(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public HostedNetworkClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public string GetHostedNetworkName()
        {
            return Channel.GetHostedNetworkName();
        }

        public void SetHostedNetworkName(string name)
        {
            Channel.SetHostedNetworkName(name);
        }

        public string GetHostedNetworkKey()
        {
            return Channel.GetHostedNetworkKey();
        }

        public void SetHostedNetworkKey(string key)
        {
            Channel.SetHostedNetworkKey(key);
        }

        public string GetHostedNetworkAuthAlgorithm()
        {
            return Channel.GetHostedNetworkAuthAlgorithm();
        }

        public void StartHostedNetwork()
        {
            Channel.StartHostedNetwork();
        }

        public int GetHostedNetworkConnectedDeviceCount()
        {
            return Channel.GetHostedNetworkConnectedDeviceCount();
        }

        public void RegisterForNewConnectedDevice()
        {
            Channel.RegisterForNewConnectedDevice();
        }

        public void UnregisterForNewConnectedDevice()
        {
            Channel.UnregisterForNewConnectedDevice();
        }

        public void StopHostedNetwork()
        {
            Channel.StopHostedNetwork();
        }
    }
}
