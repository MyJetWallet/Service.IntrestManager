namespace Service.IntrestManager.Domain.Models
{
    public class InterestRateState
    {
        public string WalletId { get; set; }
        public string AssetId { get; set; }
        public decimal TotalEarnAmount { get; set; }
        public decimal CurrentEarnAmount { get; set; }
    }
}