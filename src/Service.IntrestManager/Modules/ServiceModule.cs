using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Engines;
using Service.IntrestManager.Jobs;

namespace Service.IntrestManager.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<InterestRateByWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateByWalletNoSql.TableName);
            
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