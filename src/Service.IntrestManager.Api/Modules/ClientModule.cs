using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.AssetsDictionary.Client;
using Service.Balances.Client;
using Service.ClientWallets.Client;
using Service.ClientWallets.Domain.Models.ServiceBus;

namespace Service.IntrestManager.Api.Modules
{
    public class ClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);
            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);
            
            var spotServiceBusClient = builder
                .RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), Program.LogFactory);

            var queueName = "InterestManagerApi";
            builder.RegisterMyServiceBusSubscriberSingle<ClientWalletUpdateMessage>(spotServiceBusClient,
                ClientWalletUpdateMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
        }
    }
}