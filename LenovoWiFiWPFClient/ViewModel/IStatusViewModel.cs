using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lenovo.WiFi.Client.ViewModel
{
    public interface IStatusViewModel
    {
        string SSID { get; }

        string PresharedKey { get; }

        string Status { get; }
    }
}
