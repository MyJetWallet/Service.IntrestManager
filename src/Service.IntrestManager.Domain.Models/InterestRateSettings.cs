using System;
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
        [DataMember(Order = 6)] public decimal Apr { get; set; }
        [DataMember(Order = 7)] public DateTime LastTs { get; set; }
        [DataMember(Order = 8)] public decimal DailyLimitInUsd { get; set; }
        [DataMember(Order = 9)] public decimal Apy { get; set; }

        public static InterestRateSettings GetCopy(InterestRateSettings interestRateSettings)
        {
            return new InterestRateSettings()
            {
                Id = interestRateSettings.Id,
                WalletId = interestRateSettings.WalletId,
                Asset = interestRateSettings.Asset,
                RangeFrom = interestRateSettings.RangeFrom,
                RangeTo = interestRateSettings.RangeTo,
                Apr = interestRateSettings.Apr,
                DailyLimitInUsd = interestRateSettings.DailyLimitInUsd,
                Apy = interestRateSettings.Apy > 0 ? interestRateSettings.Apy : ConvertAprToApy(interestRateSettings.Apr)
            };
        }

        private static decimal ConvertAprToApy(decimal apr)
        {
            return apr == 0 ? 0 : Convert.ToDecimal(100 * 100 * (Math.Pow(decimal.ToDouble(1 + (apr / (100 * 100)) / 365), 365) - 1));
        } 
    }
}