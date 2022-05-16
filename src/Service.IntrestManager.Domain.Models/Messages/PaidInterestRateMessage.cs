using System;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class PaidInterestRateMessage
    {
        public const string TopicName = "paid-interest-rate";
        
        [DataMember(Order = 1)] public string TransactionId { get; set; }
        [DataMember(Order = 2)] public string BrokerId { get; set; }
        [DataMember(Order = 3)] public string WalletId { get; set; }
        [DataMember(Order = 4)] public string ClientId { get; set; }
        [DataMember(Order = 5)] public string Symbol { get; set; }
        [DataMember(Order = 6)] public DateTime Date { get; set; }
        [DataMember(Order = 7)] public decimal Amount { get; set; }
        [DataMember(Order = 8)] public decimal IndexPrice { get; set; }

    }
}