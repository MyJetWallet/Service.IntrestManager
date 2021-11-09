using System;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestRateCalculation
    {
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string Symbol { get; set; }
        [DataMember(Order = 4)] public decimal NewBalance { get; set; }
        [DataMember(Order = 5)] public decimal Apr { get; set; }
        [DataMember(Order = 6)] public decimal Amount { get; set; }
        [DataMember(Order = 7)] public DateTime Date { get; set; }
        [DataMember(Order = 8)] public long HistoryId { get; set; }
        [DataMember(Order = 9)] public DateTime LastTs { get; set; }
    }
}