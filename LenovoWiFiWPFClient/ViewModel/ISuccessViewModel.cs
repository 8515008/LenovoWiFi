using System.Windows.Input;

namespace Lenovo.WiFi.Client.ViewModel
{
    public interface ISuccessViewModel
    {
        string SSID { get; set; }

        string PresharedKey { get; set; }

        bool IsModified { get; }

        ICommand AcceptChanges { get; }
    }
}
