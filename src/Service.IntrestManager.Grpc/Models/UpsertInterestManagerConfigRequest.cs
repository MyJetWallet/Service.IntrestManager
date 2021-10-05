using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class UpsertInterestManagerConfigRequest
    {
        [DataMember(Order = 1)] public InterestManagerConfig Config { get; set; }
    }
}