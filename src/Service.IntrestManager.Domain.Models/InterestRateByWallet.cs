using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestRateByWallet
    {
        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public List<InterestRateByAsset> RateCollection { get; set; }
    }

    [DataContract]
    public class InterestRateByAsset
    {
        [DataMember(Order = 1)] public string Asset { get; set; }
        [DataMember(Order = 2)] public decimal Apy { get; set; }
    }
}