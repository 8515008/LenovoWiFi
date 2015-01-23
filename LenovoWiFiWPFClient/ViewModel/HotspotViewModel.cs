using System;
using System.Text;
using ReactiveUI;

namespace Lenovo.WiFi.Client.ViewModel
{
    class HotspotViewModel : ReactiveValidatedObject, IHotspotViewModel
    {
        private string _ssid;
        private string _presharedKey;

        private bool _valid;

        public HotspotViewModel()
        {
            ValidationObservable.Subscribe(x => this.IsValid = this.IsObjectValid());
        }

        [ValidatesViaMethod(
            AllowBlanks = false,
            AllowNull = false,
            Name = "IsSSIDValid",
            ErrorMessage = "Please enter a valid network name.")]
        public string SSID
        {
            get { return _ssid; }
            set { this.RaiseAndSetIfChanged(ref _ssid, value); }
        }

        public string AuthAlgorithm { get; set; }

        [ValidatesViaMethod(
            AllowBlanks = false,
            AllowNull = false,
            Name = "IsPresharedKeyValid",
            ErrorMessage = "Please enter a valid password.")]
        public string PresharedKey
        {
            get { return _presharedKey; }
            set { this.RaiseAndSetIfChanged(ref _presharedKey, value); }
        }

        public bool IsValid
        {
            get { return _valid; }
            set { this.RaiseAndSetIfChanged(ref _valid, value); }
        }

        public bool IsSSIDValid(string ssid)
        {
            var length = Encoding.Default.GetByteCount(ssid);
            return length >= 1 && length <= 32;
        }

        public bool IsPresharedKeyValid(string presharedKey)
        {
            var length = Encoding.Default.GetByteCount(presharedKey);
            return length >= 8 && length <= 63;
        }
    }
}
