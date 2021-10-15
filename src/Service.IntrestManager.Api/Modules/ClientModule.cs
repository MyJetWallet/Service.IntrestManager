using Autofac;
using MyJetWallet.Sdk.NoSql;

namespace Service.IntrestManager.Api.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
        }
    }
}