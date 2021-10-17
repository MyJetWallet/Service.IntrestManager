using System.Collections.Generic;
using MyNoSqlServer.Abstractions;

namespace Service.IntrestManager.Domain.Models.NoSql
{
    public class InterestRateByWalletNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-interest-rate-by-wallet";
        public static string GeneratePartitionKey() => "InterestRateByWallet";
        public static string GenerateRowKey() => "InterestRateByWallet";
        public List<InterestRateByWallet> RatesByWallets;
        public InterestRateByWallet BasicRates;
        public static InterestRateByWalletNoSql Create(List<InterestRateByWallet>  ratesByWallets, 
            InterestRateByWallet basicRates)
        {
            return new InterestRateByWalletNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                RatesByWallets = ratesByWallets,
                BasicRates = basicRates
            };
        }
    }
}