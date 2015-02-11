using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Lenovo.WiFi
{
    static class Settings
    {
        private const string KeyHostedNetworkName = "hostednetworkname";
        private const string KeyHostedNetworkKey = "hostednetworkkey";
        private static Configuration _configuration;

        public static void Load()
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public static string HostedNetworkName
        {
            get
            {
                if (string.IsNullOrEmpty(_configuration.AppSettings.Settings[KeyHostedNetworkName].Value))
                {
                    _configuration.AppSettings.Settings[KeyHostedNetworkName].Value = GenerateHostedNetworkName();
                    Save();
                }
                return _configuration.AppSettings.Settings[KeyHostedNetworkName].Value;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                if (_configuration.AppSettings.Settings[KeyHostedNetworkName].Value != value)
                {
                    _configuration.AppSettings.Settings[KeyHostedNetworkName].Value = value;
                    Save();
                }
            }
        }

        public static string HostedNetworkKey
        {
            get
            {
                if (string.IsNullOrEmpty(_configuration.AppSettings.Settings[KeyHostedNetworkKey].Value))
                {
                    _configuration.AppSettings.Settings[KeyHostedNetworkKey].Value = GenerateHostedNetworkKey();
                    Save();
                }
                return _configuration.AppSettings.Settings[KeyHostedNetworkKey].Value;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                if (_configuration.AppSettings.Settings[KeyHostedNetworkKey].Value != value)
                {
                    _configuration.AppSettings.Settings[KeyHostedNetworkKey].Value = value;
                    Save();
                }
            }
        }

        private static string GenerateHostedNetworkName()
        {
            return "LenovoWiFi" + string.Format("{0:000}", new Random().Next(1, 100));
        }

        private static string GenerateHostedNetworkKey()
        {
            return "1234567890";
        }

        public static void Save()
        {
            _configuration.Save();
        }
    }
}
