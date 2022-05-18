using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class FailedInterestRateMessage
    {
        public const string TopicName = "failed-interest-rate";

        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public DateTime Date { get; set; }
        [DataMember(Order = 3)] public string ErrorMessage { get; set; }
        [DataMember(Order = 5)] public string Symbol { get; set; }
        [DataMember(Order = 7)] public decimal Amount { get; set; }
        [DataMember(Order = 8)] public decimal IndexPrice { get; set; }

    }
}