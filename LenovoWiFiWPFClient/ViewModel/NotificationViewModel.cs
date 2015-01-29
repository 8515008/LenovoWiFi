using ReactiveUI;

namespace Lenovo.WiFi.Client.ViewModel
{
    public class NotificationViewModel : ReactiveObject, INotificationViewModel
    {

        public string Message { get; set; }
    }
}
