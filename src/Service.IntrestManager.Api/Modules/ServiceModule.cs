using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Api.Storage;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;

namespace Service.IntrestManager.Api.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();
            
            builder.RegisterMyNoSqlWriter<InterestRateSettingsNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateSettingsNoSql.TableName);
            builder.RegisterMyNoSqlWriter<InterestManagerConfigNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestManagerConfigNoSql.TableName);
            builder.RegisterMyNoSqlWriter<InterestRateByWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateByWalletNoSql.TableName);
                
            builder
                .RegisterType<InterestRateSettingsStorage>()
                .As<IInterestRateSettingsStorage>()
                .SingleInstance();
        }
    }
}