using Autofac;
using RabbitReceiver1;
using RabbitReceiver1.Autofac;

var container = ContainerConfig.Configure();

using (var scope = container.BeginLifetimeScope())
{
    var app = scope.Resolve<IApplication>();
    app.Run();
}