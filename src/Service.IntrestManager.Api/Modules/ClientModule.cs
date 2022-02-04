using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.Balances.Client;
using Service.ClientWallets.Client;

namespace Service.IntrestManager.Api.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);
            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);
        }
    }
}