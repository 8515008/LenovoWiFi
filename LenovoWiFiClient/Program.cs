using System;
using System.Timers;
using Lenovo.WiFi;

namespace LenovoWiFiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IHostedNetworkService p = new HostedNetworkProxy().Proxy;

            Timer t = new Timer();
            t.Start();

            int result = p.StartHostedNetwork();

            t.Stop();

            Console.WriteLine(t.Interval);

            t.Start();

            result = p.StopHostedNetwork();

            t.Stop();

            Console.WriteLine(t.Interval);
        }
    }
}
