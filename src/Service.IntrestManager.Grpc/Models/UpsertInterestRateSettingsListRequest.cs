using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class UpsertInterestRateSettingsListRequest
    {
        
        [DataMember(Order = 1)] public List<InterestRateSettings> InterestRateSettings { get; set; }
    }
}