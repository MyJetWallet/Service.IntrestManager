using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.ChangeBalanceGateway.Client;
using Service.IndexPrices.Client;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterSpotChangeBalanceGatewayClient(Program.Settings.ChangeBalanceGatewayGrpcServiceUrl);
            builder.RegisterIndexPricesClient(myNoSqlClient);
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);
            
            builder.RegisterMyNoSqlReader<InterestManagerConfigNoSql>(myNoSqlClient, InterestManagerConfigNoSql.TableName);
        }
    }
}