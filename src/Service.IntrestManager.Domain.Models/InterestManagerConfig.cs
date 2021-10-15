using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestManagerConfig
    {
        [DataMember(Order = 1)] public string ServiceBroker { get; set; }
        [DataMember(Order = 2)] public string ServiceClient { get; set; }
        [DataMember(Order = 3)] public string ServiceWallet { get; set; }
        [DataMember(Order = 4)] public PaidPeriod PaidPeriod { get; set; }
    }

    [DataContract]
    public enum PaidPeriod
    {
        [DataMember(Order = 1)] Day,
        [DataMember(Order = 2)] Week,
        [DataMember(Order = 3)] Month
    }
}