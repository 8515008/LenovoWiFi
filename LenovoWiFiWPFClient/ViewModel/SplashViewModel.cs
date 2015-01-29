using Autofac;
using Lenovo.WiFi.Client.Model;

namespace Lenovo.WiFi.Client.ViewModel
{
    public class SplashViewModel : ISplashViewModel
    {
        private readonly IHotspot _hotspot;

        public SplashViewModel(IHotspot hotspot)
        {
            _hotspot = hotspot;
        }

        public void Start()
        {
            _hotspot.Start();
        }
    }
}
