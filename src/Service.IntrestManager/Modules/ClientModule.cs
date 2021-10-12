using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.ChangeBalanceGateway.Client;
using Service.ClientWallets.Client;
using Service.IndexPrices.Client;

namespace Service.IntrestManager.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterSpotChangeBalanceGatewayClient(Program.Settings.ChangeBalanceGatewayGrpcServiceUrl);
            builder.RegisterIndexPricesClient(myNoSqlClient);
        }
    }
}