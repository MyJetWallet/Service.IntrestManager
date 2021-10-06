using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Client
{
    [UsedImplicitly]
    public class IntrestManagerClientFactory: MyGrpcClientFactory
    {
        public IntrestManagerClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IInterestRateSettingsService GetInterestRateSettingsService() => CreateGrpcService<IInterestRateSettingsService>();
        public IInterestManagerConfigService GetInterestManagerConfigService() => CreateGrpcService<IInterestManagerConfigService>();
        public IInterestManagerService GetInterestManagerService() => CreateGrpcService<IInterestManagerService>();
    }
}
