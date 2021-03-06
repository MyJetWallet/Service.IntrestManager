using System;
using System.Runtime.Serialization;

namespace Service.IntrestManager.Domain.Models
{
    [DataContract]
    public class CalculationHistory
    {
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public DateTime CalculationDate { get; set; }
        [DataMember(Order = 3)] public DateTime CompletedDate { get; set; }
        [DataMember(Order = 4)] public int WalletCount { get; set; }
        [DataMember(Order = 5)] public double AmountInWalletsInUsd { get; set; }
        [DataMember(Order = 6)] public double CalculatedAmountInUsd { get; set; }
        [DataMember(Order = 7)] public string SettingsJson { get; set; }
        [DataMember(Order = 8)] public DateTime LastTs { get; set; }
    }
}