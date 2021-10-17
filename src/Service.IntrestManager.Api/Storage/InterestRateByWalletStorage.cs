using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;

namespace Service.IntrestManager.Api.Storage
{
    public class InterestRateByWalletStorage : IInterestRateByWalletStorage, IStartable
    {
        private readonly ILogger<InterestRateByWalletStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> _writer;

        public InterestRateByWalletStorage(IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> writer, 
            ILogger<InterestRateByWalletStorage> logger)
        {
            _writer = writer;
            _logger = logger;
        }

        public async Task<InterestRateByWallet> GetRatesByWallet(string walletId)
        {
            var cache = (await _writer.GetAsync()).FirstOrDefault();
            var rates = cache?.RatesByWallets ?? new List<InterestRateByWallet>();
            
            var ratesByWallet = rates.FirstOrDefault(e => e.WalletId == walletId);
            var ratesByWalletCount = ratesByWallet?.RateCollection?.Count ?? 0;
            _logger.LogInformation("Find {ratesCount} rates for {walletId}. Json: {ratesJson}", 
                ratesByWalletCount, walletId, JsonConvert.SerializeObject(ratesByWallet));
            
            if (ratesByWalletCount != 0)
            {
                return ratesByWallet;
            }
            // todo: делать ли поправку на текущий баланс
            var basicRates = cache?.BasicRates ?? new InterestRateByWallet();
            return basicRates;
        }

        public async Task UpdateRates(List<InterestRateSettings> settings)
        {
            _logger.LogInformation("UpdateRates in InterestRateByWalletStorage run with settings : {settingsJson}",
                JsonConvert.SerializeObject(settings));
            
            // todo: implement
        }

        public void Start()
        {
            _logger.LogInformation("InterestRateByWalletStorage start caching.");
            var cache = _writer.GetAsync().GetAwaiter().GetResult();
            _logger.LogInformation($"InterestRateByWalletStorage finish caching {cache.Count()} records.");
        }
    }
}