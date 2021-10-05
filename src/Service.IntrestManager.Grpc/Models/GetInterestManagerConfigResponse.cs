using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class GetInterestManagerConfigResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public InterestManagerConfig Config { get; set; }
    }
}