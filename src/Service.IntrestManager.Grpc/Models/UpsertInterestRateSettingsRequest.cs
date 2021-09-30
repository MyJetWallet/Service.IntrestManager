using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class UpsertInterestRateSettingsRequest
    {
        [DataMember(Order = 1)] public InterestRate InterestRate { get; set; }
    }
}