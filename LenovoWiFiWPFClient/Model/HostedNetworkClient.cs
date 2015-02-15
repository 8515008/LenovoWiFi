using System.ServiceModel;
using System.ServiceModel.Channels;

using NLog;

namespace Lenovo.WiFi.Client.Model
{
    public class HostedNetworkClient : DuplexClientBase<IHostedNetworkService>, IHostedNetworkService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Trace("GetHostedNetworkName: Invoked");
            return Channel.GetHostedNetworkName();
        }

        public void SetHostedNetworkName(string name)
        {
            Logger.Trace("SetHostedNetworkName: Invoked with parameter: {0}", name);
            Channel.SetHostedNetworkName(name);
        }

        public string GetHostedNetworkKey()
        {
            Logger.Trace("GetHostedNetworkKey: Invoked");
            return Channel.GetHostedNetworkKey();
        }

        public void SetHostedNetworkKey(string key)
        {
            Logger.Trace("SetHostedNetworkKey: Invoked with parameter: {0}", key);
            Channel.SetHostedNetworkKey(key);
        }

        public string GetHostedNetworkAuthAlgorithm()
        {
            Logger.Trace("GetHostedNetworkAuthAlgorithm: Invoked");
            return Channel.GetHostedNetworkAuthAlgorithm();
        }

        public void StartHostedNetwork()
        {
            Logger.Trace("StartHostedNetwork: Invoked");
            Channel.StartHostedNetwork();
        }

        public int GetHostedNetworkConnectedDeviceCount()
        {
            Logger.Trace("GetHostedNetworkConnectedDeviceCount: Invoked");
            return Channel.GetHostedNetworkConnectedDeviceCount();
        }

        public void RegisterForNewConnectedDevice()
        {
            Logger.Trace("RegisterForNewConnectedDevice: Invoked");
            Channel.RegisterForNewConnectedDevice();
        }

        public void UnregisterForNewConnectedDevice()
        {
            Logger.Trace("UnregisterForNewConnectedDevice: Invoked");
            Channel.UnregisterForNewConnectedDevice();
        }

        public void StopHostedNetwork()
        {
            Logger.Trace("StopHostedNetwork: Invoked");
            Channel.StopHostedNetwork();
        }
    }
}
