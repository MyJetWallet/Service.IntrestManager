using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestRate
    {
        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public string Asset { get; set; }
        [DataMember(Order = 3)] public decimal RangeFrom { get; set; }
        [DataMember(Order = 4)] public decimal RangeTo { get; set; }
        [DataMember(Order = 5)] public string Apy { get; set; }
    }
}