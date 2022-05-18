using System;
using System.Runtime.Serialization;
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
        [OperationContract]
        Task<RetryPaidByCreatedDateResponse> RetryPaidByCreatedDateAsync(RetryPaidByCreatedDateRequest request);
        [OperationContract]
        Task<GetPaidExpectedDateResponse> GetPaidExpectedDateAsync(GetPaidExpectedDateRequest request);
        [OperationContract]
        Task<GetPaidExpectedAmountResponse> GetPaidExpectedAmountAsync(GetPaidExpectedAmountRequest request);
    }

    [DataContract]
    public class RetryPaidByCreatedDateResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }

    [DataContract]
    public class RetryPaidByCreatedDateRequest
    {
        [DataMember(Order = 1)] public DateTime CreatedDate { get; set; }
    }
}