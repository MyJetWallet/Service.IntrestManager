using System;

namespace Service.IntrestManager.Domain.Models
{
    public class InterestRateCalculation
    {
        public long Id { get; set; }
        public string WalletId { get; set; }
        public string Symbol { get; set; }
        public decimal NewBalance { get; set; }
        public decimal Apy { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}