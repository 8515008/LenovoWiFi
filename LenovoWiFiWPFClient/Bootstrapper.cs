using Lenovo.WiFi.Client.Model;
using Lenovo.WiFi.Client.View;
using Lenovo.WiFi.Client.ViewModel;

using Autofac;

namespace Lenovo.WiFi.Client
{
    internal class Bootstrapper
    {
        public IContainer Build()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<HostedNetwork>().As<IHotspot>().SingleInstance();

            builder.RegisterType<SplashWindow>().SingleInstance();
            builder.RegisterType<SplashViewModel>().As<ISplashViewModel>().SingleInstance();
            builder.RegisterType<SuccessWindow>().SingleInstance();
            builder.RegisterType<SuccessViewModel>().As<ISuccessViewModel>().SingleInstance();
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<MainViewModel>().As<IMainViewModel>().SingleInstance();
            builder.RegisterType<StatusWindow>().SingleInstance();
            builder.RegisterType<StatusViewModel>().As<IStatusViewModel>().SingleInstance();
            builder.RegisterType<NotificationWindow>();

            return builder.Build();
        }
    }
}
