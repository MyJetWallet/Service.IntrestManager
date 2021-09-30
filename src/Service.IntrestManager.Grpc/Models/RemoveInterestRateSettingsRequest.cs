using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class RemoveInterestRateSettingsRequest
    {
        [DataMember(Order = 1)] public InterestRateSettings InterestRateSettings { get; set; }
    }
}