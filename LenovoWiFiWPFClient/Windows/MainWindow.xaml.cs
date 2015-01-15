using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessagingToolkit.QRCode.Codec;

namespace Lenovo.WiFi.Client.Windows
{
    public partial class MainWindow : BottomRightWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeString();
        }

        private void InitializeString()
        {
            const string format = "http://www.lenovo.com/lenovowifi/welcome.htm?T={0}&P={1}&S={2}";

            var client = ((App) Application.Current).Client;
            var authAlgorithm = client.GetHostedNetworkAuthAlgorithm();
            var ssid = client.GetHostedNetworkName();
            var key = client.GetHostedNetworkKey();

            var qrString = string.Format(format, authAlgorithm, key, ssid);
            var encoder = new QRCodeEncoder();
            var bitmap = encoder.Encode(qrString);
            this.QRCode.Source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            this.TextBoxWiFiName.Text = ssid;
            this.TextBoxWiFiKey.Text = key;
        }
    }
}
