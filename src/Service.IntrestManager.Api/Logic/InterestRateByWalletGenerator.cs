using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;

namespace Service.IntrestManager.Api.Logic
{
    public class InterestRateByWalletGenerator : IInterestRateByWalletGenerator
    {
        private readonly ILogger<InterestRateByWalletGenerator> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> _ratesWriter;
        private readonly IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> _settingWriter;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly DatabaseContextFactory _contextFactory;

        public InterestRateByWalletGenerator(IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> ratesWriter, 
            ILogger<InterestRateByWalletGenerator> logger, 
            IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> settingWriter, 
            IWalletBalanceService walletBalanceService, 
            IAssetsDictionaryClient assetsDictionaryClient, 
            DatabaseContextFactory contextFactory)
        {
            _ratesWriter = ratesWriter;
            _logger = logger;
            _settingWriter = settingWriter;
            _walletBalanceService = walletBalanceService;
            _assetsDictionaryClient = assetsDictionaryClient;
            _contextFactory = contextFactory;
        }
        
        public async Task<InterestRateByWallet> GenerateRatesByWallet(string walletId)
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
            foreach (var asset in assets.Select(e => e.Symbol).Distinct())
            {
                var settingByAssetAndEmptyWallet = settings
                    .Where(e => string.IsNullOrEmpty(e.WalletId) && e.Asset == asset)
                    .ToList();

                if (settingByAssetAndEmptyWallet.Any())
                {
                    SetRatesByAsset(settingByAssetAndEmptyWallet, ratesByWallet, asset, balances);
                }
            }

            // stage 4 : rates without settings
            SetZeroRates(assets, ratesByWallet);
            
            // stage 4 : set accumulated rates
            await SetAccumulatedRates(ratesByWallet);

            await SaveToNoSql(ratesByWallet);

            return ratesByWallet;
        }

        private async Task SetAccumulatedRates(InterestRateByWallet ratesByWallet)
        {
            await using var ctx = _contextFactory.Create();
            var accumulatedRate = ctx.GetAccumulatedRates(ratesByWallet.WalletId);
            
            ratesByWallet.RateCollection.ForEach(e =>
            {
                e.AccumulatedAmount = accumulatedRate.TryGetValue(e.Asset, out var amount) 
                    ? amount 
                    : 0;
            });
        }

        private async Task SaveToNoSql(InterestRateByWallet ratesByWallet)
        {
            await _ratesWriter.InsertOrReplaceAsync(InterestRateByWalletNoSql.Create(ratesByWallet));
        }

        private static void SetZeroRates(IReadOnlyList<IAsset> assets, InterestRateByWallet ratesByWallet)
        {
            foreach (var asset in assets.Select(e => e.Symbol).Distinct())
            {
                var rateExist = ratesByWallet.RateCollection.Any(e => e.Asset == asset);
                if (!rateExist)
                {
                    ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                    {
                        Asset = asset,
                        Apr = 0,
                        Apy = 0
                    });
                }
            }
        }

        private static void SetRatesByAsset(IReadOnlyCollection<InterestRateSettings> settingByAssetAndEmptyWallet, 
            InterestRateByWallet ratesByWallet, string asset, WalletBalanceList balances)
        {
            var rateExist = ratesByWallet.RateCollection.Any(e => e.Asset == asset);
            if (rateExist) 
                return;
            
            if (settingByAssetAndEmptyWallet.Count == 1)
            {
                ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                {
                    Asset = asset,
                    Apr = settingByAssetAndEmptyWallet.First().Apr,
                    Apy = ConvertAprToApy(settingByAssetAndEmptyWallet.First().Apr)
                });
            }
            else
            {
                SetRatesBySettingsWithRange(settingByAssetAndEmptyWallet, ratesByWallet, asset, balances);
            }
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
                    Apr = settingForWalletAndAsset.First().Apr,
                    Apy = ConvertAprToApy(settingForWalletAndAsset.First().Apr)
                });
            }
            else
            {
                SetRatesBySettingsWithRange(settingForWalletAndAsset, ratesByWallet, asset, balances);
            }
        }

        private static void SetRatesBySettingsWithRange(IEnumerable<InterestRateSettings> settings,
            InterestRateByWallet ratesByWallet, string asset, WalletBalanceList balances)
        {
            var balanceEntity = balances.Balances?.FirstOrDefault(e => e.AssetId == asset);
            var balance = 0m;
            if (balanceEntity != null)
            {
                balance = (decimal) balanceEntity.Balance;
            }

            foreach (var s in settings)
            {
                if (balance >= s.RangeFrom && balance < s.RangeTo)
                {
                    ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                    {
                        Asset = asset,
                        Apr = s.Apr,
                        Apy = ConvertAprToApy(s.Apr)
                    });
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
                        Apr = settingByWallet.Apr,
                        Apy = ConvertAprToApy(settingByWallet.Apr)
                    });
                }
            }
        }

        public async Task ClearRates()
        {
            _logger.LogInformation("ClearRates run in InterestRateByWalletGenerator");
            await _ratesWriter.CleanAndKeepMaxPartitions(0);
        }

        private static decimal ConvertAprToApy(decimal apr)
        {
            return apr == 0 ? 0 : Convert.ToDecimal(100 * 100 * (Math.Pow(decimal.ToDouble(1 + (apr / (100 * 100)) / 365), 365) - 1));
        } 
    }
}