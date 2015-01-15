using System;
using System.ServiceModel;

namespace Lenovo.WiFi
{
    [ServiceBehavior]
    public class HostedNetworkService : IHostedNetworkService, IDisposable
    {
        private bool disposed;
        readonly HostedNetworkManager _hostedNetworkManager = new HostedNetworkManager();

        public HostedNetworkService()
        {
            SetHostedNetworkName(GenerateWiFiName());
            SetHostedNetworkKey(GenerateWiFiKey());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _hostedNetworkManager.Dispose();
            }

            disposed = true;
        }

        private string GenerateWiFiName()
        {
            return "LenovoWiFi" + string.Format("{0:000}", new Random().Next(1, 100));
        }

        private string GenerateWiFiKey()
        {
            return "1234567890";
        }

        public string GetHostedNetworkName()
        {
            return _hostedNetworkManager.GetHostedNetworkName();
        }

        public void SetHostedNetworkName(string name)
        {
            _hostedNetworkManager.SetHostedNetworkName(name);
        }

        public string GetHostedNetworkKey()
        {
            return _hostedNetworkManager.GetHostedNetworkKey();
        }

        public void SetHostedNetworkKey(string key)
        {
            _hostedNetworkManager.SetHostedNetworkKey(key);
        }

        public string GetHostedNetworkAuthAlgorithm()
        {
            return _hostedNetworkManager.GetHostedNetworkAuthAlgorithm();
        }

        public void StartHostedNetwork()
        {
            if (_hostedNetworkManager.IsHostedNetworkAllowed)
            {
                _hostedNetworkManager.StartHostedNetwork();
            }
        }

        public void StopHostedNetwork()
        {
            if (_hostedNetworkManager.IsHostedNetworkAllowed)
            {
                _hostedNetworkManager.StopHostedNetwork();
            }
        }
    }
}
