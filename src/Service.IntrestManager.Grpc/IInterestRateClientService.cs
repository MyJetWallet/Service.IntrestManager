using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc
{
    [ServiceContract]
    public interface IInterestRateClientService
    {
        [OperationContract]
        Task<GetInterestRatesByWalletResponse> GetInterestRatesByWalletAsync(GetInterestRatesByWalletRequest request);
    }

    [DataContract]
    public class GetInterestRatesByWalletRequest
    {
        [DataMember(Order = 1)] public string WalletId { get; set; }
    }

    [DataContract]
    public class GetInterestRatesByWalletResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public InterestRateByWallet Rates { get; set; }
    }
}