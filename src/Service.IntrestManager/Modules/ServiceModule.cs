using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Engines;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Jobs;
using Service.IntrestManager.Services;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();
            
            builder.RegisterMyNoSqlWriter<InterestRateSettingsNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateSettingsNoSqlEntity.TableName);
            builder.RegisterMyNoSqlWriter<InterestManagerConfigNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestManagerConfigNoSql.TableName);

            builder
                .RegisterType<InterestRateSettingsStorage>()
                .As<IInterestRateSettingsStorage>()
                .SingleInstance();
            builder
                .RegisterType<InterestManagerConfigService>()
                .As<IInterestManagerConfigService>();

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
        }
    }
}