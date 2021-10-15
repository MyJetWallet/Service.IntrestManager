using Autofac;
using Service.InterestManager.Postrges;

namespace Service.IntrestManager.Api.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();
        }
    }
}