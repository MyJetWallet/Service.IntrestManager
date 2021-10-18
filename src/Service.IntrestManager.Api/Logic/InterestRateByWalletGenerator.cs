using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
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

        public InterestRateByWalletGenerator(IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> ratesWriter, 
            ILogger<InterestRateByWalletGenerator> logger, 
            IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> settingWriter, 
            IWalletBalanceService walletBalanceService, 
            IAssetsDictionaryClient assetsDictionaryClient)
        {
            _ratesWriter = ratesWriter;
            _logger = logger;
            _settingWriter = settingWriter;
            _walletBalanceService = walletBalanceService;
            _assetsDictionaryClient = assetsDictionaryClient;
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

            await SaveToNoSql(ratesByWallet);

            return ratesByWallet;
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
                        Apy = 0
                    });
                }
            }
        }

        private static void SetRatesByAsset(IReadOnlyCollection<InterestRateSettings> settingByAssetAndEmptyWallet, 
            InterestRateByWallet ratesByWallet, string asset, WalletBalanceList balances)
        {
            if (settingByAssetAndEmptyWallet.Count == 1)
            {
                var rateExist = ratesByWallet.RateCollection.Any(e => e.Asset == asset);
                if (!rateExist)
                {
                    ratesByWallet.RateCollection.Add(new InterestRateByAsset()
                    {
                        Asset = asset,
                        Apy = settingByAssetAndEmptyWallet.First().Apy
                    });
                }
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
            var balanceEntity = balances.Balances.FirstOrDefault(e => e.AssetId == asset);
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
                        Apy = settingByWallet.Apy
                    });
                }
            }
        }

        public async Task ClearRates()
        {
            _logger.LogInformation("ClearRates run in InterestRateByWalletGenerator");
            await _ratesWriter.CleanAndKeepMaxPartitions(0);
        }
    }
}