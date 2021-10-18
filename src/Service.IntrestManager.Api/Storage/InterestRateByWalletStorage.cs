using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;

namespace Service.IntrestManager.Api.Storage
{
    public class InterestRateByWalletStorage : IInterestRateByWalletStorage, IStartable
    {
        private readonly ILogger<InterestRateByWalletStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> _writer;
        private readonly IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> _settingWriter;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;

        public InterestRateByWalletStorage(IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> writer, 
            ILogger<InterestRateByWalletStorage> logger, 
            IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> settingWriter, 
            IWalletBalanceService walletBalanceService, 
            IAssetsDictionaryClient assetsDictionaryClient)
        {
            _writer = writer;
            _logger = logger;
            _settingWriter = settingWriter;
            _walletBalanceService = walletBalanceService;
            _assetsDictionaryClient = assetsDictionaryClient;
        }

        public async Task<InterestRateByWallet> GetRatesByWallet(string walletId)
        {
            var cache = (await _writer.GetAsync(walletId)).FirstOrDefault();

            if (cache != null)
                return cache.Rates;
            
            var rates = await GenerateRatesByWallet(walletId);
            await _writer.InsertOrReplaceAsync(InterestRateByWalletNoSql.Create(rates));
            return rates;
        }

        private async Task<InterestRateByWallet> GenerateRatesByWallet(string walletId)
        {
            var settingsNoSql = (await _settingWriter.GetAsync()).ToList();
            var settings = settingsNoSql
                .Select(e => e.InterestRateSettings)
                .ToList();
            
            var assets = _assetsDictionaryClient.GetAllAssets();
            
            var balances = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = walletId,
                Symbol = string.Empty
            });

            var ratesByWallet = new InterestRateByWallet()
            {
                WalletId = walletId,
                RateCollection = new List<InterestRateByAsset>()
            };
            
            // stage 1 : wallet + asset
            foreach (var asset in assets.Select(e => e.Symbol).Distinct())
            {
                var settingForWalletAndAsset = settings
                    .Where(e => e.WalletId == walletId && e.Asset == asset)
                    .ToList();

                if (settingForWalletAndAsset.Any())
                {
                    SetRatesByWalletAndAsset(settingForWalletAndAsset, ratesByWallet, asset, balances);
                }
            }
            
            // stage 2 : wallet + empty asset
            var settingByWallet = settings
                .Where(e => e.WalletId == walletId && string.IsNullOrEmpty(e.Asset))
                .ToList();

            if (settingByWallet.Any())
            {
                SetRatesByWallet(settingByWallet, assets, ratesByWallet, settingsNoSql);
            }
            
            // stage 3 : empty wallet + asset

            return ratesByWallet;
        }

        private static void SetRatesByWallet(IReadOnlyCollection<InterestRateSettings> settingByWallet, 
            IEnumerable<IAsset> assets, InterestRateByWallet ratesByWallet,
            IEnumerable<InterestRateSettingsNoSql> settingsNoSql)
        {
            if (settingByWallet.Count == 1)
            {
                SetRatesByWalletAndEmptyAsset(assets, ratesByWallet, settingByWallet.First());
            }
            else
            {
                var mostNewSettingsByWallet = settingsNoSql
                    .Where(e => settingByWallet.Select(x => x.Id).Contains(e.InterestRateSettings.Id))
                    .OrderByDescending(e => e.TimeStamp)
                    .First()
                    .InterestRateSettings;

                SetRatesByWalletAndEmptyAsset(assets, ratesByWallet, mostNewSettingsByWallet);
            }
        }

        private static void SetRatesByWalletAndAsset(IReadOnlyCollection<InterestRateSettings> settingForWalletAndAsset, 
            InterestRateByWallet ratesByWallet,
            string asset, 
            WalletBalanceList balances)
        {
            if (settingForWalletAndAsset.Count == 1)
            {
                ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                {
                    Asset = asset,
                    Apy = settingForWalletAndAsset.First().Apy
                });
            }
            else
            {
                var balanceEntity = balances.Balances.FirstOrDefault(e => e.AssetId == asset);
                var balance = 0m;
                if (balanceEntity != null)
                {
                    balance = (decimal) balanceEntity.Balance;
                }

                foreach (var s in settingForWalletAndAsset)
                {
                    if (balance >= s.RangeFrom && balance < s.RangeTo)
                    {
                        ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                        {
                            Asset = asset,
                            Apy = s.Apy
                        });
                    }
                }
            }
        }

        private static void SetRatesByWalletAndEmptyAsset(IEnumerable<IAsset> assets, InterestRateByWallet ratesByWallet,
            InterestRateSettings settingByWallet)
        {
            foreach (var asset in assets.Select(e => e.Symbol).Distinct())
            {
                var rateExist = ratesByWallet.RateCollection.Any(e => e.Asset == asset);
                if (!rateExist)
                {
                    ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                    {
                        Asset = asset,
                        Apy = settingByWallet.Apy
                    });
                }
            }
        }

        public async Task ClearRates()
        {
            _logger.LogInformation("ClearRates run in InterestRateByWalletStorage");
            await _writer.CleanAndKeepMaxPartitions(0);
        }

        public void Start()
        {
            _logger.LogInformation("InterestRateByWalletStorage start caching.");
            var cache = _writer.GetAsync().GetAwaiter().GetResult();
            _logger.LogInformation($"InterestRateByWalletStorage finish caching {cache.Count()} records.");
        }
    }
}