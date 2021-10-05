using System.ServiceModel;
using System.Threading.Tasks;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Grpc
{
    [ServiceContract]
    public interface IInterestManagerConfigService
    {
        [OperationContract]
        Task<GetInterestManagerConfigResponse> GetInterestManagerConfigAsync();
        [OperationContract]
        Task<UpsertInterestManagerConfigResponse> UpsertInterestManagerConfigAsync(UpsertInterestManagerConfigRequest request);
    }
}