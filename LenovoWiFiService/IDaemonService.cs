using System.ServiceModel;

namespace Lenovo.WiFi
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDaemonService" in both code and config file together.
    [ServiceContract]
    public interface IDaemonService
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
