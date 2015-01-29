using Lenovo.WiFi.Client.Model;

using ReactiveUI;

namespace Lenovo.WiFi.Client.ViewModel
{
    public class StatusViewModel : ReactiveObject, IStatusViewModel
    {
        private const string StatusTextFormat = "当前连接数：{0}";

        private readonly IHotspot _hotspot;

        private readonly ObservableAsPropertyHelper<string> _ssid;
        private readonly ObservableAsPropertyHelper<string> _presharedKey;
        private readonly ObservableAsPropertyHelper<string> _status;

        public StatusViewModel(IHotspot hotspot)
        {
            _hotspot = hotspot;

            this.WhenAnyValue(x => x._hotspot.SSID).ToProperty(this, x => x.SSID, out _ssid);
            this.WhenAnyValue(x => x._hotspot.PresharedKey).ToProperty(this, x => x.PresharedKey, out _presharedKey);
            this.WhenAny(x => x._hotspot.ConnectedDeviceCount, count => string.Format(StatusTextFormat, count.Value))
                .ToProperty(this, x => x.Status, out _status);
        }

        public string SSID
        {
            get { return _ssid.Value; }
        }

        public string PresharedKey
        {
            get { return _presharedKey.Value; }
        }

        public string Status
        {
            get { return _status.Value; }
        }
    }
}
