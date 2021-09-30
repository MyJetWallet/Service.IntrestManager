using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<InterestRateNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                InterestRateNoSqlEntity.TableName);
        }
    }
}