using System.ServiceModel;
using System.Threading.Tasks;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}