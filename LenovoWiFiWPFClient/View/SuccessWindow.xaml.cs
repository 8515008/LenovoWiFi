using Lenovo.WiFi.Client.Model;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lenovo.WiFi.Client.View
{
    public partial class SuccessWindow : BottomRightWindow
    {
        public SuccessWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
