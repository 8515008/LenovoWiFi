using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using MessagingToolkit.QRCode.Codec;
using Lenovo.WiFi.Client.Model;

namespace Lenovo.WiFi.Client.View
{
    public partial class MainWindow : BottomRightWindow
    {
        private int Count = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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
