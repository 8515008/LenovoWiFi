using System.Windows.Media;

namespace Lenovo.WiFi.Client.ViewModel
{
    public interface IMainViewModel
    {
        string SSID { get; }

        string PresharedKey { get; }

        ImageSource QRCode { get; }
    }
}
