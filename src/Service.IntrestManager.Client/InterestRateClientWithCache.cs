using System;
using System.Linq;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Client
{
    public class InterestRateClientWithCache : IInterestRateClientWithCache
    {
        private readonly IMyNoSqlServerDataReader<InterestRateByWalletNoSql> _reader;
        private readonly IInterestRateClientService _interestRateClientService;

        public InterestRateClientWithCache(IMyNoSqlServerDataReader<InterestRateByWalletNoSql> reader,
            IInterestRateClientService interestRateClientService)
        {
            _reader = reader;
            _interestRateClientService = interestRateClientService;
        }

        public async Task<InterestRateByWallet> GetInterestRatesByWalletAsync(string walletId)
        {
            Console.WriteLine($"GetInterestRatesByWalletAsync start ({walletId}) {DateTime.UtcNow:O}");
            try
            {
                var cache = _reader.Get(InterestRateByWalletNoSql.GeneratePartitionKey(walletId)).FirstOrDefault();
                if (cache != null && cache.Rates.RateCollection.All(t=>t.NextPaymentDate > DateTime.UtcNow))
                    return cache.Rates;

                var ratesFromGrpc = await _interestRateClientService.GetInterestRatesByWalletAsync(
                    new GetInterestRatesByWalletRequest()
                    {
                        WalletId = walletId
                    });
                return ratesFromGrpc.Rates;
            }
            finally
            {
                Console.WriteLine($"GetInterestRatesByWalletAsync stop ({walletId}) {DateTime.UtcNow:O}");
            }
            
        }
    }
}