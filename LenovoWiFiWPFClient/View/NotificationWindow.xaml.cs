using System;
using System.Windows.Threading;

namespace Lenovo.WiFi.Client.View
{
    public partial class NotificationWindow : BottomRightWindow
    {
        private DispatcherTimer _timer;

        public NotificationWindow()
        {
            InitializeComponent();

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += (sender, args) => this.Close();
            _timer.Interval = new TimeSpan(0, 0, 5);
            _timer.Start();
        }
    }
}
