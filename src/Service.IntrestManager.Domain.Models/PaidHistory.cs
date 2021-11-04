using System;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class PaidHistory
    {
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public DateTime CreatedDate { get; set; }
        [DataMember(Order = 3)] public DateTime RangeFrom { get; set; }
        [DataMember(Order = 4)] public DateTime RangeTo { get; set; }
        [DataMember(Order = 5)] public int WalletCount { get; set; }
        [DataMember(Order = 6)] public double TotalPaidInUsd { get; set; }
        [DataMember(Order = 7)] public DateTime LastTs { get; set; }
    }
}