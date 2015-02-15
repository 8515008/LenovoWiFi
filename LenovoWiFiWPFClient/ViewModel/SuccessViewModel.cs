using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Lenovo.WiFi.Client.Model;

using ReactiveUI;

namespace Lenovo.WiFi.Client.ViewModel
{
    public class SuccessViewModel : ReactiveObject, ISuccessViewModel
    {
        private readonly IHotspot _hotspot;

        private string _currentSSID;
        private string _currentPresharedKey;

        private string _ssid;
        private string _presharedKey;

        private readonly ObservableAsPropertyHelper<bool> _modified;

        public SuccessViewModel(IHotspot hotspot)
        {
            _hotspot = hotspot;
            this.CurrentSSID = this.SSID = _hotspot.SSID;
            this.CurrentPresharedKey = this.PresharedKey = _hotspot.PresharedKey;

            var dirty = this.WhenAny(x => x.SSID, x => x.PresharedKey, x => x.CurrentSSID, x => x.CurrentPresharedKey,
                (ssid, psk, cssid, cpsk) => ssid.Value != cssid.Value || psk.Value != cpsk.Value);

            var valid = this.WhenAny(x => x.SSID, x => x.PresharedKey,
                (name, key) => IsSSIDValid(name.Value) && IsPresharedKeyValid(key.Value));

            var canAcceptChanges = Observable.CombineLatest(dirty, valid, (isDirty, isValid) => isDirty && isValid);
            canAcceptChanges.ToProperty(this, x => x.IsModified, out _modified);

            AcceptChanges = ReactiveCommand.CreateAsyncTask(
                canAcceptChanges,
                _ => Task.Run(() =>
                {
                    _hotspot.Stop();

                    _hotspot.SSID = this.SSID;
                    _hotspot.PresharedKey = this.PresharedKey;
                    this.CurrentSSID = this.SSID;
                    this.CurrentPresharedKey = this.PresharedKey;

                    _hotspot.Start();
                }));
        }

        public string CurrentSSID
        {
            get { return _currentSSID; }
            set { this.RaiseAndSetIfChanged(ref _currentSSID, value); }
        }

        public string CurrentPresharedKey
        {
            get { return _currentPresharedKey; }
            set { this.RaiseAndSetIfChanged(ref _currentPresharedKey, value); }
        }

        public string SSID
        {
            get { return _ssid; }
            set { this.RaiseAndSetIfChanged(ref _ssid, value); }
        }

        public string PresharedKey
        {
            get { return _presharedKey; }
            set { this.RaiseAndSetIfChanged(ref _presharedKey, value); }
        }

        public bool IsModified { get { return _modified.Value; } }

        public ICommand AcceptChanges { get; protected set; }

        private static bool IsSSIDValid(string ssid)
        {
            var length = Encoding.Default.GetByteCount(ssid);
            return length >= 1 && length <= 32;
        }

        private static bool IsPresharedKeyValid(string presharedKey)
        {
            var length = Encoding.Default.GetByteCount(presharedKey);
            return length >= 8 && length <= 63;
        }
    }
}
