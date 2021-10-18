using Autofac;
using MyJetWallet.Sdk.ServiceBus;
using Service.IntrestManager.Domain.Models;
using ApplicationEnvironment = MyJetWallet.Sdk.Service.ApplicationEnvironment;

namespace Service.IntrestManager.Modules
{
    public class ServiceBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort), Program.LogFactory);
            builder.RegisterMyServiceBusPublisher<PaidInterestRateMessage>(serviceBusClient, PaidInterestRateMessage.TopicName, true);
        }
    }
}