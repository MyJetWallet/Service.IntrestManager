using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    public class FailedInterestRateMessage
    {
        public const string TopicName = "failed-interest-rate";

        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public DateTime Date { get; set; }
        [DataMember(Order = 3)] public string ErrorMessage { get; set; }
    }
}