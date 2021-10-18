using Autofac;
using MyNoSqlServer.DataReader;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.IntrestManager.Client
{
    public static class AutofacHelper
    {
        public static void RegisterInterestManagerClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new IntrestManagerClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetInterestRateSettingsService()).As<IInterestRateSettingsService>().SingleInstance();
            builder.RegisterInstance(factory.GetInterestManagerConfigService()).As<IInterestManagerConfigService>().SingleInstance();
            builder.RegisterInstance(factory.GetInterestManagerService()).As<IInterestManagerService>().SingleInstance();
        }
        
        public static void RegisterInterestRateClientWithCache(this ContainerBuilder builder, 
            string grpcServiceUrl, IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            var factory = new IntrestManagerClientFactory(grpcServiceUrl);
            var reader =  new MyNoSqlReadRepository<InterestRateByWalletNoSql>(myNoSqlSubscriber, InterestRateByWalletNoSql.TableName);
            
            builder.RegisterInstance(factory.GetInterestRateClientService()).As<IInterestRateClientService>().SingleInstance();
            builder
                .RegisterInstance(new InterestRateClientWithCache(reader, factory.GetInterestRateClientService()))
                .As<IInterestRateClientWithCache>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
