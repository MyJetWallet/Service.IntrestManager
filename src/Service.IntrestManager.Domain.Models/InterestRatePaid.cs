using System;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class InterestRatePaid
    {
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string Symbol { get; set; }
        [DataMember(Order = 4)] public DateTime Date { get; set; }
        [DataMember(Order = 5)] public decimal Amount { get; set; }
        [DataMember(Order = 6)] public PaidState State { get; set; }
        [DataMember(Order = 7)] public string ErrorMessage { get; set; }
        [DataMember(Order = 8)] public string TransactionId { get; set; }
    }

    [DataContract]
    public enum PaidState
    {
        [DataMember(Order = 1)] Undefined,
        [DataMember(Order = 2)] New,
        [DataMember(Order = 3)] Completed,
        [DataMember(Order = 4)] Failed
    }
}