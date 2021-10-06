using Autofac;
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
    }
}
