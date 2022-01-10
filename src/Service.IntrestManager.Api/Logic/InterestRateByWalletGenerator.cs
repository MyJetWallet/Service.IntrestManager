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
        private readonly IMyNoSqlServerDataWriter<InterestManagerConfigNoSql> _configWriter;

        public InterestRateByWalletGenerator(IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> ratesWriter, 
            ILogger<InterestRateByWalletGenerator> logger, 
            IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> settingWriter, 
            IWalletBalanceService walletBalanceService, 
            IAssetsDictionaryClient assetsDictionaryClient, 
            DatabaseContextFactory contextFactory, 
            IMyNoSqlServerDataWriter<InterestManagerConfigNoSql> configWriter)
        {
            _ratesWriter = ratesWriter;
            _logger = logger;
            _settingWriter = settingWriter;
            _walletBalanceService = walletBalanceService;
            _assetsDictionaryClient = assetsDictionaryClient;
            _contextFactory = contextFactory;
            _configWriter = configWriter;
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
            
            
            //stage 5 : set earn state and date
            await AddInterestRateState(ratesByWallet);
            await AddNextPaymentDate(ratesByWallet);
            
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
                    Apy = settingByAssetAndEmptyWallet.First().Apy
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
                    Apy = settingForWalletAndAsset.First().Apy
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
                        Apy = s.Apy
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
                        Apy = settingByWallet.Apy
                    });
                }
            }
        }


        private async Task AddInterestRateState(InterestRateByWallet ratesByWallet)
        {
            await using var ctx = _contextFactory.Create();
            var states = await ctx.GetEarnStates(ratesByWallet.WalletId);

            foreach (var rate in ratesByWallet.RateCollection)
            {
                if (!states.TryGetValue(rate.Asset, out var values)) continue;
                
                rate.CurrentEarnAmount = values.current;
                rate.TotalEarnAmount = values.total;
            }
        }

        private async Task AddNextPaymentDate(InterestRateByWallet ratesByWallet)
        {
            var serviceConfig = (await _configWriter.GetAsync()).FirstOrDefault();
            var nextPayment = serviceConfig?.Config.PaidPeriod switch
            {
                PaidPeriod.Day => DateTime.UtcNow.Date.AddDays(1),
                PaidPeriod.Week => GetNextMonday(),
                _ => GetFirstDayOfNextMonth()
            };
            
            foreach (var rate in ratesByWallet.RateCollection)
            {
                rate.NextPaymentDate = nextPayment;
            }
            
            //locals
            DateTime GetNextMonday()
            {
                
                var diff = DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday;
                if (diff < 0)
                    diff += 7;
                diff += 7;
                var nextMonday = DateTime.UtcNow.AddDays(diff).Date;
                return nextMonday;
            }

            DateTime GetFirstDayOfNextMonth()
            {
                var tempDate = DateTime.Now.AddMonths(1);
                return new DateTime(tempDate.Year, tempDate.Month, 1).Date; 
            }
        }

        public async Task ClearRates()
        {
            _logger.LogInformation("ClearRates run in InterestRateByWalletGenerator");
            var entities = await _ratesWriter.GetAsync();
            foreach (var key in entities.Select(t=>t.PartitionKey).Distinct())
            {
                await _ratesWriter.CleanAndKeepMaxRecords(key, 0);
            }
        }
    }
}