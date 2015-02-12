using System;
using System.Collections.Generic;
using System.ServiceModel;
using NLog;

namespace Lenovo.WiFi
{
    [ServiceBehavior]
    public class HostedNetworkService : IHostedNetworkService, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HostedNetworkManager _hostedNetworkManager;
        private readonly IList<IHostedNetworkServiceCallback> _clients = new List<IHostedNetworkServiceCallback>();

        public HostedNetworkService()
        {
            Logger.Trace(".ctor: Invoked");
            _hostedNetworkManager = new HostedNetworkManager();

            Logger.Trace(".ctor: Applying settings");
            ApplySettings();
            Logger.Trace(".ctor: Registering event handlers");
            RegisterEventHandlers();
        }

        private void ApplySettings()
        {
            _hostedNetworkManager.SetHostedNetworkName(Settings.Instance.HostedNetworkName);
            _hostedNetworkManager.SetHostedNetworkKey(Settings.Instance.HostedNetworkKey);
        }

        private void RegisterEventHandlers()
        {
            _hostedNetworkManager.DeviceConnected += (sender, args) =>
            {
                foreach (var client in _clients)
                {
                    client.DeviceConnected(args.PhysicalAddress);
                }
            };
        }

        public void Dispose()
        {
            _hostedNetworkManager.Dispose();
        }

        public string GetHostedNetworkName()
        {
            return _hostedNetworkManager.GetHostedNetworkName();
        }

        public void SetHostedNetworkName(string name)
        {
            _hostedNetworkManager.SetHostedNetworkName(name);
            Settings.Instance.HostedNetworkName = name;
        }

        public string GetHostedNetworkKey()
        {
            return _hostedNetworkManager.GetHostedNetworkKey();
        }

        public void SetHostedNetworkKey(string key)
        {
            _hostedNetworkManager.SetHostedNetworkKey(key);
            Settings.Instance.HostedNetworkKey = key;
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

        public void RegisterForNewConnectedDevice()
        {
            var client = OperationContext.Current.GetCallbackChannel<IHostedNetworkServiceCallback>();

            if (client != null)
            {
                this._clients.Add(client);
            }
        }

        public void UnregisterForNewConnectedDevice()
        {
            var client = OperationContext.Current.GetCallbackChannel<IHostedNetworkServiceCallback>();

            if (client != null && this._clients.Contains(client))
            {
                this._clients.Remove(client);
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
