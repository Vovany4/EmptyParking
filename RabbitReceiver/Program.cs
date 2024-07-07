using Autofac;
using RabbitReceiver;
using RabbitReceiver.Autofac;

var container = ContainerConfig.Configure();

using (var scope = container.BeginLifetimeScope())
{
    var app = scope.Resolve<IApplication>();
    app.Run();
}