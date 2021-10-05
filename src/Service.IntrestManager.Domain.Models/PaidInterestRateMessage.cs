using System;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class PaidInterestRateMessage
    {
        public const string TopicName = "paid-interest-rate";
        
        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public string Symbol { get; set; }
        [DataMember(Order = 3)] public DateTime Date { get; set; }
        [DataMember(Order = 4)] public decimal Amount { get; set; }
    }
}