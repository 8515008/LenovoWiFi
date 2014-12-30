using System.Windows;
using System.Windows.Input;

namespace Lenovo.WiFi.Client.Windows
{
    public partial class SuccessWindow : BottomRightWindow
    {
        private bool _editing = false;

        private string _name = string.Empty;
        private string _key = string.Empty;

        public SuccessWindow()
        {
            InitializeComponent();

            InitializeUI();
        }

        private void InitializeUI()
        {
            var app = (App)Application.Current;

            this.TextBoxWiFiName.Text = _name = app.Client.GetHostedNetworkName();
            this.TextBoxWiFiKey.Text = _key = app.Client.GetHostedNetworkKey();
        }

        private void WindowClose(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Modify(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;

            if (_editing)
            {
                this.ButtonModify.Content = app.Resources["Modify"];

                if (this.TextBoxWiFiName.Text != _name)
                {
                    app.Client.SetHostedNetworkName(_name);
                    _name = this.TextBoxWiFiName.Text;
                }

                if (this.TextBoxWiFiKey.Text != _key)
                {
                    app.Client.SetHostedNetworkKey(_key);
                    _key = this.TextBoxWiFiKey.Text;
                }

                this.TextBoxWiFiName.IsEnabled = false;
                this.TextBoxWiFiKey.IsEnabled = false;
            }
            else
            {
                this.ButtonModify.Content = app.Resources["Modify"];

                this.TextBoxWiFiName.IsEnabled = true;
                this.TextBoxWiFiKey.IsEnabled = true;
            }
        }
    }
}
