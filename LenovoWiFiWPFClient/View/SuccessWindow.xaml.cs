using Lenovo.WiFi.Client.Model;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lenovo.WiFi.Client.View
{
    public partial class SuccessWindow : BottomRightWindow
    {
        private int Count = 0;
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
            if (Count++ % 2 == 0)
            {
                App.DeskBandPipe.SendCommandToDeskband(DeskbandCommand.ICS_off);
            }
            else
            {
                App.DeskBandPipe.SendCommandToDeskband(DeskbandCommand.ICS_on);
            }
        }
    }
}
