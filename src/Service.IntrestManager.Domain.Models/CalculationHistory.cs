using System;

namespace Service.IntrestManager.Domain.Models
{
    public class CalculationHistory
    {
        public long Id { get; set; }
        public DateTime CompletedDate { get; set; }
        public int WalletCount { get; set; }
        public decimal AmountInWalletsInUsd { get; set; }
        public decimal CalculatedAmountInUsd { get; set; }
    }
}