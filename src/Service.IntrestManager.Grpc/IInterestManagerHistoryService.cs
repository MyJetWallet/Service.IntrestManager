using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc
{
    public interface IInterestManagerService
    {
        [OperationContract]
        Task<GetCalculationHistoryResponse> GetCalculationHistoryAsync();
        [OperationContract]
        Task<GetPaidHistoryResponse> GetPaidHistoryAsync();
    }

    [DataContract]
    public class GetPaidHistoryResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public List<PaidHistory> History { get; set; }
    }
    
    [DataContract]
    public class GetCalculationHistoryResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public List<CalculationHistory> History { get; set; }
    }
}