using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Grpc.Models
{
    [DataContract]
    public class GetCalculationsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public List<InterestRateCalculation> Calculations { get; set; }
        [DataMember(Order = 4)] public long IdForNextQuery { get; set; }
    }
}