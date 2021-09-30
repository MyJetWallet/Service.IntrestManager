using MyNoSqlServer.Abstractions;

namespace Service.IntrestManager.Domain.Models
{
    public class InterestRateSettingsNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-interest-rate-settings";
        public static string GeneratePartitionKey(string walletId, string asset) => walletId + "-" + asset;

        public static string GenerateRowKey(decimal rangeFrom, decimal rangeTo) => rangeFrom + "-" + rangeTo;

        public InterestRateSettings InterestRateSettings;
        
        public static InterestRateSettingsNoSqlEntity Create(InterestRateSettings entity)
        {
            return new InterestRateSettingsNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.WalletId, entity.Asset),
                RowKey = GenerateRowKey(entity.RangeFrom, entity.RangeTo),
                InterestRateSettings = entity
            };
        }
    }
}