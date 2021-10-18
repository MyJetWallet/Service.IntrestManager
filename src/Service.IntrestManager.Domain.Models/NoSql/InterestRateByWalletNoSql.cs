using System.Collections.Generic;
using MyNoSqlServer.Abstractions;

namespace Service.IntrestManager.Domain.Models.NoSql
{
    public class InterestRateByWalletNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-interest-rate-by-wallet";
        public static string GeneratePartitionKey(string walletId) => walletId;
        public static string GenerateRowKey() => "InterestRateByWallet";
        public InterestRateByWallet Rates;
        public static InterestRateByWalletNoSql Create(InterestRateByWallet entity)
        {
            return new InterestRateByWalletNoSql()
            {
                PartitionKey = GeneratePartitionKey(entity.WalletId),
                RowKey = GenerateRowKey(),
                Rates = entity
            };
        }
    }
}