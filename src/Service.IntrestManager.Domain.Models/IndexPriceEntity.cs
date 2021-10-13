namespace Service.IntrestManager.Domain.Models
{
    public class IndexPriceEntity
    {
        public long Id { get; set; }
        public string Asset { get; set; }
        public decimal PriceInUsd { get; set; }
    }
}