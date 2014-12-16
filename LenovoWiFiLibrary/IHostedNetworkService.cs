using System.ServiceModel;

namespace Lenovo.WiFi
{
    [ServiceContract]
    public interface IHostedNetworkService
    {
        [OperationContract]
        int GetHostedNetworkName(out string name);

        [OperationContract]
        int SetHostedNetworkName(string name);

        [OperationContract]
        int GetHostedNetworkKey(out string key);

        [OperationContract]
        int SetHostedNetworkKey(string key);

        [OperationContract]
        int StartHostedNetwork();

        [OperationContract]
        int StopHostedNetwork();
    }
}
