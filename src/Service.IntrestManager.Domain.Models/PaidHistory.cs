using System;

namespace Service.IntrestManager.Domain.Models
{
    public class PaidHistory
    {
        public long Id { get; set; }
        public DateTime CompletedDate { get; set; }
        public DateTime RangeFrom { get; set; }
        public DateTime RangeTo { get; set; }
        public int WalletCount { get; set; }
        public decimal TotalPaidInUsd { get; set; }
    }
}