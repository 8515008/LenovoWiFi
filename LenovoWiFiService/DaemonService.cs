﻿using System;

namespace Lenovo.WiFi
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DaemonService" in both code and config file together.
    public class DaemonService : IDaemonService
    {
        readonly HostedNetworkManager _hostedNetworkManager = new HostedNetworkManager();

        public int GetHostedNetworkName(out string name)
        {
            name = _hostedNetworkManager.GetHostedNetworkName();

            return 0;
        }

        public int SetHostedNetworkName(string name)
        {
            var result = 0;

            try
            {
                _hostedNetworkManager.SetHostedNetworkName(name);
            }
            catch (Exception)
            {
                result = 1;
            }

            return result;
        }

        public int GetHostedNetworkKey(out string key)
        {
            key = _hostedNetworkManager.GetHostedNetworkKey();

            return 0;
        }

        public int SetHostedNetworkKey(string key)
        {
            var result = 0;

            try
            {
                _hostedNetworkManager.SetHostedNetworkKey(key);
            }
            catch (Exception)
            {
                result = 1;
            }

            return result;
        }

        public int StartHostedNetwork()
        {
            var result = 0;

            try
            {
                if (_hostedNetworkManager.IsHostedNetworkAllowed)
                {
                    _hostedNetworkManager.StartHostedNetwork();
                }
            }
            catch (Exception)
            {
                result = 1;
            }

            return result;
        }

        public int StopHostedNetwork()
        {
            var result = 0;

            try
            {
                if (_hostedNetworkManager.IsHostedNetworkAllowed)
                {
                    _hostedNetworkManager.StopHostedNetwork();
                }
            }
            catch (Exception)
            {
                result = 1;
            }

            return result;
        }
    }
}