using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lenovo.WiFi.Client.View
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

                this.TextBoxWiFiName.IsEnabled = false;
                this.TextBoxWiFiKey.IsEnabled = false;

                var task = new Task(() =>
                {
                    app.Client.StopHostedNetwork();

                    if (this.TextBoxWiFiName.Text != _name)
                    {
                        _name = this.TextBoxWiFiName.Text;
                        app.Client.SetHostedNetworkName(_name);
                    }

                    if (this.TextBoxWiFiKey.Text != _key)
                    {
                        _key = this.TextBoxWiFiKey.Text;
                        app.Client.SetHostedNetworkKey(_key);
                    }

                    app.Client.StartHostedNetwork();
                });

                task.Start(TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                this.ButtonModify.Content = app.Resources["Save"];

                this.TextBoxWiFiName.IsEnabled = true;
                this.TextBoxWiFiKey.IsEnabled = true;
            }

            _editing = !_editing;
        }
    }
}
