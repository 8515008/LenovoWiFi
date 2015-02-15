using System;
using System.Windows;
using System.Windows.Media;

namespace Lenovo.WiFi.Client.View
{
    public class BottomRightWindow : Window
    {
        private const int MarginRight = 20;
        private const int MarginBottom = 10;

        public BottomRightWindow()
        {
            this.BorderBrush = new SolidColorBrush(SystemColors.ActiveBorderColor);
            this.BorderThickness = new Thickness(1);

            this.ResizeMode = ResizeMode.NoResize;
            this.ShowInTaskbar = false;
            this.Topmost = true;

            this.Loaded += OnLoaded;
            this.Deactivated += OnDeactivated;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - MarginRight;
            this.Top = desktopWorkingArea.Bottom - this.Height - MarginBottom;
        }

        private void OnDeactivated(object sender, EventArgs eventArgs)
        {
            this.Hide();
        }
    }
}
