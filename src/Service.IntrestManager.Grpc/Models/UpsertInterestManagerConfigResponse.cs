using System.Runtime.Serialization;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class UpsertInterestManagerConfigResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
}