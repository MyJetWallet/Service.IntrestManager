using System.ServiceModel;
using System.Threading.Tasks;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Grpc
{
    [ServiceContract]
    public interface IInterestManagerService
    {
        [OperationContract]
        Task<GetCalculationHistoryResponse> GetCalculationHistoryAsync();
        [OperationContract]
        Task<GetPaidHistoryResponse> GetPaidHistoryAsync();
        [OperationContract]
        Task<GetCalculationsResponse> GetCalculationsAsync(GetCalculationsRequest request);
        [OperationContract]
        Task<GetPaidResponse> GetPaidAsync(GetPaidRequest request);
    }
}