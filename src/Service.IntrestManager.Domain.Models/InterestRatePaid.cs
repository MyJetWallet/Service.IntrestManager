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
        [DataMember(Order = 9)] public long HistoryId { get; set; }
        [DataMember(Order = 10)] public DateTime LastTs { get; set; }
        [DataMember(Order = 11)] public long Iteration { get; set; }
        [DataMember(Order = 12)] public DateTime DatePaid { get; set; }
        [DataMember(Order = 13)] public decimal IndexPrice { get; set; }
    }

    [DataContract]
    public enum PaidState
    {
        [DataMember(Order = 1)] Undefined,
        [DataMember(Order = 2)] New,
        [DataMember(Order = 3)] Completed,
        [DataMember(Order = 4)] Failed,
        [DataMember(Order = 5)] Retry,
        [DataMember(Order = 6)] TooLowAmount,
    }
}