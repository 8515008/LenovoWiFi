using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lenovo.WiFi.Client.Model
{
    class HostedNetwork
    {
        internal bool IsAllowed { get; set; }
        internal string SSID { get; set; }
        internal string PresharedKey { get; set; }
        internal string Authentication { get; set; }
        internal string Cipher { get; set; }
        internal bool IsStarted { get; set; }
    }
}
