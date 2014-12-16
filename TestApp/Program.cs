using System;
using Lenovo.WiFi;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new HostedNetworkManager();
            manager.SetHostedNetworkName("Lenovo Wi-Fi");
            Console.WriteLine(manager.GetHostedNetworkName());

            manager.SetHostedNetworkKey("Aa123456");
            Console.WriteLine(manager.GetHostedNetworkKey());

            manager.StartHostedNetwork();
            
            manager.StopHostedNetwork();
        }
    }
}
