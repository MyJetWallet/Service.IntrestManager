using System;

namespace Service.IntrestManager.Domain.Models
{
    public class InterestRatePaid
    {
        public long Id { get; set; }
        public string WalletId { get; set; }
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public PaidState State { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum PaidState
    {
        New,
        Completed,
        Failed
    }
}