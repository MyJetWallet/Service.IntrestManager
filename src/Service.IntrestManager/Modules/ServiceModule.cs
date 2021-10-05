using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Jobs;
using Service.IntrestManager.Services;

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
                .RegisterType<InterestCalculationJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            builder
                .RegisterType<PaidCalculationJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            builder
                .RegisterType<InterestProcessingJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}