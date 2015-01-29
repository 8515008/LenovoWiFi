using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Lenovo.WiFi.Client.Model;

using MessagingToolkit.QRCode.Codec;
using ReactiveUI;
using Autofac;

namespace Lenovo.WiFi.Client.ViewModel
{
    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        private const string QRCodeContentFormat = "http://www.lenovo.com/lenovowifi/welcome.htm?T={0}&P={1}&S={2}";

        private readonly IHotspot _hotspot;

        private readonly ObservableAsPropertyHelper<string> _ssid;
        private readonly ObservableAsPropertyHelper<string> _presharedKey;
        private readonly ObservableAsPropertyHelper<ImageSource> _qrCode;

        public MainViewModel(IHotspot hotspot)
        {
            _hotspot = hotspot;

            this.WhenAnyValue(x => x._hotspot.SSID).ToProperty(this, x => x.SSID, out _ssid);
            this.WhenAnyValue(x => x._hotspot.PresharedKey).ToProperty(this, x => x.PresharedKey, out _presharedKey);
            this.WhenAny(
                x => x._hotspot.Authentication,
                x => x._hotspot.PresharedKey,
                x => x._hotspot.SSID,
                (authAlgo, psk, ssid) =>
                {
                    var qrString = string.Format(QRCodeContentFormat, authAlgo.Value, psk.Value, ssid.Value);
                    var encoder = new QRCodeEncoder();
                    var bitmap = encoder.Encode(qrString);
                    return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }).ToProperty(this, x => x.QRCode, out _qrCode);
        }

        public string SSID
        {
            get { return _ssid.Value; }
        }

        public string PresharedKey
        {
            get { return _presharedKey.Value; }
        }

        public ImageSource QRCode
        {
            get { return _qrCode.Value; }
        }
    }
}
