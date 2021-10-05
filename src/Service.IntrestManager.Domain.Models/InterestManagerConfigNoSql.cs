using MyNoSqlServer.Abstractions;

namespace Service.IntrestManager.Domain.Models
{
    public class InterestManagerConfigNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-interest-manager-config";
        public static string GeneratePartitionKey() => "InterestManagerConfig";
        public static string GenerateRowKey() => "InterestManagerConfig";
        public InterestManagerConfig Config;
        public static InterestManagerConfigNoSql Create(InterestManagerConfig entity)
        {
            return new InterestManagerConfigNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Config = entity
            };
        }
    }
}