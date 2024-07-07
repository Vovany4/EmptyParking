using Autofac;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;

namespace RabbitReceiver.Autofac
{
    public static class ContainerConfig
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Application>().As<IApplication>();
            builder.RegisterType<MainRepository>().As<IMainRepository>();
            builder.RegisterType<MainService>().As<IMainService>();

            return builder.Build();
        }
    }
}
