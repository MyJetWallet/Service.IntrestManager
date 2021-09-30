using System.ServiceModel;
using System.Threading.Tasks;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Grpc
{
    [ServiceContract]
    public interface IInterestRateSettingsService
    {
        [OperationContract]
        Task<GetInterestRateSettingsResponse> GetInterestRateSettingsAsync();
        [OperationContract]
        Task<UpsertInterestRateSettingsResponse> UpsertInterestRateSettingsAsync(UpsertInterestRateSettingsRequest request);
        [OperationContract]
        Task<RemoveInterestRateSettingsResponse> RemoveInterestRateSettingsAsync(RemoveInterestRateSettingsRequest request);
    }
}