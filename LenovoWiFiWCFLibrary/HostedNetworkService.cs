using System;
using System.Collections.Specialized;
using System.Configuration;
using System.ServiceModel;

namespace Lenovo.WiFi
{
    [ServiceBehavior]
    public class HostedNetworkService : IHostedNetworkService, IDisposable
    {
        bool _disposed;
        readonly HostedNetworkManager _hostedNetworkManager = new HostedNetworkManager();
        readonly NameValueCollection _appSettings = ConfigurationManager.AppSettings;

        public HostedNetworkService()
        {
            Settings.Load();
            SetHostedNetworkName(Settings.HostedNetworkName);
            SetHostedNetworkKey(Settings.HostedNetworkKey);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _hostedNetworkManager.Dispose();
            }

            _disposed = true;
        }

        public string GetHostedNetworkName()
        {
            return _hostedNetworkManager.GetHostedNetworkName();
        }

        public void SetHostedNetworkName(string name)
        {
            Settings.HostedNetworkName = name;
            _hostedNetworkManager.SetHostedNetworkName(name);
        }

        public string GetHostedNetworkKey()
        {
            return _hostedNetworkManager.GetHostedNetworkKey();
        }

        public void SetHostedNetworkKey(string key)
        {
            Settings.HostedNetworkKey = key;
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

        public int GetHostedNetworkConnectedDeviceCount()
        {
            return _hostedNetworkManager.ConnectedDeviceCount;
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
