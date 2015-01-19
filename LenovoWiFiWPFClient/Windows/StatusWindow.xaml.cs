using System.Windows;
using System.Xml.Schema;

namespace Lenovo.WiFi.Client.Windows
{
    /// <summary>
    /// Interaction logic for WiFiDetails.xaml
    /// </summary>
    public partial class StatusWindow : BottomRightWindow
    {
        public StatusWindow()
        {
            InitializeComponent();
            InitializeString();
        }

        private void InitializeString()
        {
            var client = ((App)Application.Current).Client;
            var authAlgorithm = client.GetHostedNetworkAuthAlgorithm();
            var ssid = client.GetHostedNetworkName();
            var key = client.GetHostedNetworkKey();

            this.LabelWiFiNameValue.Content = ssid;
            this.LabelWiFiKeyValue.Content = key;
            this.LabelStatus.Content = string.Format("当前连接数: {0}", client.GetHostedNetworkConnectedDeviceCount());
        }
    }
}
