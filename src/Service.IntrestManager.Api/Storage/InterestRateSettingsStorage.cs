using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;

namespace Service.IntrestManager.Api.Storage
{
    public class InterestRateSettingsStorage : IInterestRateSettingsStorage
    {
        private readonly ILogger<InterestRateSettingsStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> _interestRateWriter;
        private readonly DatabaseContextFactory _contextFactory;
        private readonly IInterestRateByWalletGenerator _interestRateByWalletGenerator;

        public InterestRateSettingsStorage(IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> interestRateWriter,
            DatabaseContextFactory contextFactory, 
            ILogger<InterestRateSettingsStorage> logger, 
            IInterestRateByWalletGenerator interestRateByWalletGenerator)
        {
            _interestRateWriter = interestRateWriter;
            _contextFactory = contextFactory;
            _logger = logger;
            _interestRateByWalletGenerator = interestRateByWalletGenerator;
        }

        public async Task SyncSettings()
        {
            await using var ctx = _contextFactory.Create();
            var settings = ctx.GetSettings();

            var noSqlSettings = settings.Select(InterestRateSettingsNoSql.Create).ToList();

            await _interestRateWriter.CleanAndKeepMaxPartitions(0);
            await _interestRateWriter.BulkInsertOrReplaceAsync(noSqlSettings);

           await _interestRateByWalletGenerator.ClearRates();
        }

        public async Task<List<InterestRateSettings>> GetSettings()
        {
            try
            {
                await using var ctx = _contextFactory.Create();
                return ctx.GetSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new List<InterestRateSettings>();
            }
        }

        public async Task<string> UpsertSettings(InterestRateSettings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.Asset) &&
                    string.IsNullOrWhiteSpace(settings.WalletId))
                {
                    return "Wallet and asset cannot be empty.";
                }
                if (string.IsNullOrWhiteSpace(settings.Asset))
                {
                    settings.RangeFrom = 0;
                    settings.RangeTo = 0;
                    settings.Asset = string.Empty;
                }
                if (string.IsNullOrWhiteSpace(settings.WalletId))
                {
                    settings.WalletId = string.Empty;
                }
                await using var ctx = _contextFactory.Create();
                var dbSettingsCollection = ctx.GetSettings();
                
                var validateResult = settings.Id == 0 
                    ? await GetValidateResult(settings, dbSettingsCollection)
                    : SettingsValidationResultEnum.Ok;
                switch (validateResult)
                {
                    case SettingsValidationResultEnum.Ok:
                    {
                        await ctx.UpsertSettings(settings);
                        await SyncSettings();
                        return string.Empty;
                    }
                    case SettingsValidationResultEnum.CrossedRangeError:
                        return "CrossedRangeError";
                    case SettingsValidationResultEnum.DoubleWalletSettingsError:
                        return "DoubleWalletSettingsError";
                    default:
                        return "Smth wrong in InterestRateSettingsStorage.UpsertSettings";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.Message;
            }
        }

        public async Task<List<SettingsValidationResult>> UpsertSettingsList(List<InterestRateSettings> settingsList)
        {
            var validationResult = new List<SettingsValidationResult>();
            await using var ctx = _contextFactory.Create();
            var dbSettingsCollection = ctx.GetSettings();
            foreach (var settings in settingsList)
            {
                if (string.IsNullOrWhiteSpace(settings.Asset) &&
                    string.IsNullOrWhiteSpace(settings.WalletId))
                {
                    validationResult.Add(new SettingsValidationResult()
                    {
                        InterestRateSettings = settings,
                        ValidationResult = SettingsValidationResultEnum.WalletAndAssetCannotBeEmpty
                    });
                    continue;
                }
                if (string.IsNullOrWhiteSpace(settings.Asset))
                {
                    settings.RangeFrom = 0;
                    settings.RangeTo = 0;
                    settings.Asset = string.Empty;
                }
                if (string.IsNullOrWhiteSpace(settings.WalletId))
                {
                    settings.WalletId = string.Empty;
                }

                var newSettingsWithoutCurrent = settingsList.Where(e => e != settings);
                var allSettingsWithoutCurrent = dbSettingsCollection.Select(InterestRateSettings.GetCopy).ToList();
                allSettingsWithoutCurrent.AddRange(newSettingsWithoutCurrent);
                
                validationResult.Add(new SettingsValidationResult()
                {
                    InterestRateSettings = settings,
                    ValidationResult = await GetValidateResult(settings, allSettingsWithoutCurrent)
                });
            }
            if (validationResult.All(e => e.ValidationResult == SettingsValidationResultEnum.Ok))
            {
                await ctx.UpsertSettingsList(settingsList);
                await SyncSettings();
            }
            return validationResult;
        }

        private async Task<SettingsValidationResultEnum> GetValidateResult(InterestRateSettings settings,
            IReadOnlyCollection<InterestRateSettings> settingsCollection)
        {
            if (!string.IsNullOrWhiteSpace(settings.Asset) &&
                !string.IsNullOrWhiteSpace(settings.WalletId))
            {
                var settingsWithWalletAndAsset = settingsCollection
                    .Where(e => e.Asset == settings.Asset &&
                                e.WalletId == settings.WalletId &&
                                e.Id != settings.Id)
                    .ToList();

                if (settingsWithWalletAndAsset.Any())
                {
                    var settingsInRange = settingsWithWalletAndAsset
                        .Where(e => (settings.RangeFrom > e.RangeFrom && settings.RangeFrom < e.RangeTo) ||
                                    (settings.RangeTo > e.RangeFrom && settings.RangeTo < e.RangeTo) ||
                                    (settings.RangeFrom < e.RangeFrom && settings.RangeTo > e.RangeTo));
                    if (settingsInRange.Any())
                    {
                        return SettingsValidationResultEnum.CrossedRangeError;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(settings.Asset))
            {
                var clone = settingsCollection
                    .FirstOrDefault(e => e.WalletId == settings.WalletId &&
                                         (string.IsNullOrWhiteSpace(e.Asset) || e.Asset == null) &&
                                         e.Id != settings.Id);
                if (clone != null)
                {
                    return SettingsValidationResultEnum.DoubleWalletSettingsError;
                }
            }
            if (string.IsNullOrWhiteSpace(settings.WalletId))
            {
                var settingsWithAsset = settingsCollection
                    .Where(e => e.Asset == settings.Asset &&
                                (string.IsNullOrWhiteSpace(e.WalletId) || e.WalletId == null) &&
                                e.Id != settings.Id)
                    .ToList();
                if (settingsWithAsset.Any())
                {
                    var settingsInRange = settingsWithAsset
                        .Where(e => (settings.RangeFrom > e.RangeFrom && settings.RangeFrom < e.RangeTo) ||
                                    (settings.RangeTo > e.RangeFrom && settings.RangeTo < e.RangeTo) ||
                                    (settings.RangeFrom < e.RangeFrom && settings.RangeTo > e.RangeTo));
                    if (settingsInRange.Any())
                    {
                        return SettingsValidationResultEnum.CrossedRangeError;
                    }
                }
            }
            return SettingsValidationResultEnum.Ok;
        }

        public async Task RemoveSettings(InterestRateSettings settings)
        {
            try
            {
                await using var ctx = _contextFactory.Create();
                await ctx.RemoveSettings(settings);

                await SyncSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}