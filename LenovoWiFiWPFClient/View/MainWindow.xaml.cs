using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using MessagingToolkit.QRCode.Codec;
using Lenovo.WiFi.Client.Model;

namespace Lenovo.WiFi.Client.View
{
    public partial class MainWindow : BottomRightWindow
    {
        private int i = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if(i++%2 == 0 )
                App.DeskBandPipe.SendCommandToDeskband(DeskbandCommand.ICS_on);
            else 
                App.DeskBandPipe.SendCommandToDeskband(DeskbandCommand.ICS_off);
        }
    }
}
