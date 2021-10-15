using Autofac;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Engines;
using Service.IntrestManager.Jobs;

namespace Service.IntrestManager.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();

            builder
                .RegisterType<InterestManagerJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<InterestCalculationEngine>()
                .AsSelf()
                .SingleInstance();
            builder
                .RegisterType<PaidCalculationEngine>()
                .AsSelf()
                .SingleInstance();
            builder
                .RegisterType<InterestProcessingEngine>()
                .AsSelf()
                .SingleInstance();
            builder
                .RegisterType<IndexPriceEngine>()
                .AsSelf()
                .SingleInstance();
        }
    }
}