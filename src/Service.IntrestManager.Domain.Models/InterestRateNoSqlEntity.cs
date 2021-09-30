using MyNoSqlServer.Abstractions;

namespace Service.IntrestManager.Domain.Models
{
    public class InterestRateNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-interest-rate";
        public static string GeneratePartitionKey(string walletId, string asset) => walletId + "-" + asset;

        public static string GenerateRowKey(decimal rangeFrom, decimal rangeTo) => rangeFrom + "-" + rangeTo;

        public InterestRate InterestRate;
        
        public static InterestRateNoSqlEntity Create(InterestRate entity)
        {
            return new InterestRateNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.WalletId, entity.Asset),
                RowKey = GenerateRowKey(entity.RangeFrom, entity.RangeTo),
                InterestRate = entity
            };
        }
    }
}