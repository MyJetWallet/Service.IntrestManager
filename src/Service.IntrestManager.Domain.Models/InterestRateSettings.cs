using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestRateSettings
    {
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string Asset { get; set; }
        [DataMember(Order = 4)] public decimal RangeFrom { get; set; }
        [DataMember(Order = 5)] public decimal RangeTo { get; set; }
        [DataMember(Order = 6)] public decimal Apy { get; set; }

        public static InterestRateSettings GetCopy(InterestRateSettings interestRateSettings)
        {
            return new InterestRateSettings()
            {
                Id = interestRateSettings.Id,
                WalletId = interestRateSettings.WalletId,
                Asset = interestRateSettings.Asset,
                RangeFrom = interestRateSettings.RangeFrom,
                RangeTo = interestRateSettings.RangeTo,
                Apy = interestRateSettings.Apy
            };
        }
    }
}