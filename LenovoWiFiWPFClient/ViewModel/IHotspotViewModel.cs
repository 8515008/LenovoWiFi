using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lenovo.WiFi.Client.ViewModel
{
    interface IHotspotViewModel
    {
        string SSID { get; set; }
        string AuthAlgorithm { get; set; }
        string PresharedKey { get; set; }
    }
}
