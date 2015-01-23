using System.Net.NetworkInformation;
using System.ServiceModel;

namespace Lenovo.WiFi
{
    [ServiceContract]
    public interface IHostedNetworkServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void DeviceConnected(byte[] macAddress);
    }
}
