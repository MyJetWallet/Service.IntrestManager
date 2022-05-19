using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Api.Jobs;
using Service.IntrestManager.Api.Logic;
using Service.IntrestManager.Api.Services;
using Service.IntrestManager.Api.Storage;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Api.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);

            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();
            
            builder.RegisterMyNoSqlWriter<InterestRateSettingsNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateSettingsNoSql.TableName);
            builder.RegisterMyNoSqlWriter<InterestManagerConfigNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestManagerConfigNoSql.TableName);
            builder.RegisterMyNoSqlWriter<InterestRateByWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateByWalletNoSql.TableName);
            builder.RegisterMyNoSqlReader<InterestManagerConfigNoSql>(myNoSqlClient, InterestManagerConfigNoSql.TableName);

            builder
                .RegisterType<InterestRateSettingsStorage>()
                .As<IInterestRateSettingsStorage>()
                .SingleInstance();
            builder
                .RegisterType<InterestRateByWalletGenerator>()
                .As<IInterestRateByWalletGenerator>()
                .SingleInstance();
            builder
                .RegisterType<InterestRateSettingsService>()
                .As<IInterestRateSettingsService>()
                .SingleInstance();

            builder
                .RegisterType<WalletUpdateJob>()
                .AsSelf()
                .SingleInstance()
                .AutoActivate();
        }
    }
}