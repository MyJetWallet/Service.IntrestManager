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

            builder.RegisterInstance(factory.GetHelloService()).As<IInterestRateSettingsService>().SingleInstance();
        }
    }
}
