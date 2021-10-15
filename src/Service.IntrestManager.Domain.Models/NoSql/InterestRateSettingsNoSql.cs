using MyNoSqlServer.Abstractions;

namespace Service.IntrestManager.Domain.Models.NoSql
{
    public class InterestRateSettingsNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-interest-rate-settings";
        public static string GeneratePartitionKey(string walletId, string asset) => walletId + "-" + asset;

        public static string GenerateRowKey(decimal rangeFrom, decimal rangeTo) => rangeFrom + "-" + rangeTo;

        public InterestRateSettings InterestRateSettings;
        
        public static InterestRateSettingsNoSql Create(InterestRateSettings entity)
        {
            return new InterestRateSettingsNoSql()
            {
                PartitionKey = GeneratePartitionKey(entity.WalletId, entity.Asset),
                RowKey = GenerateRowKey(entity.RangeFrom, entity.RangeTo),
                InterestRateSettings = entity
            };
        }
    }
}