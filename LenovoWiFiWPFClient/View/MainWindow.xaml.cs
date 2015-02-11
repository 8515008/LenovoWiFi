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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


    }
}
