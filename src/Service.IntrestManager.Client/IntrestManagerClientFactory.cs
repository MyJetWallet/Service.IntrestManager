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

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
