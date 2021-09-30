using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Services
{
    public class InterestRateSettingsStorage : IInterestRateSettingsStorage
    {
        private readonly ILogger<InterestRateSettingsStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestRateSettingsNoSqlEntity> _interestRateWriter;
        private readonly DatabaseContextFactory _contextFactory;

        public InterestRateSettingsStorage(IMyNoSqlServerDataWriter<InterestRateSettingsNoSqlEntity> interestRateWriter,
            DatabaseContextFactory contextFactory, 
            ILogger<InterestRateSettingsStorage> logger)
        {
            _interestRateWriter = interestRateWriter;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<InterestRateSettings>> GetSettings()
        {
            try
            {
                var rates = (await _interestRateWriter.GetAsync()).ToList();
                if (rates.Any())
                {
                    return rates.Select(e => e.InterestRateSettings).ToList();
                }
                await using var ctx = _contextFactory.Create();
                return ctx.GetSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new List<InterestRateSettings>();
            }
        }

        public async Task UpsertSettings(InterestRateSettings settings)
        {
            try
            {
                await using var ctx = _contextFactory.Create();
                await ctx.UpsertSettings(settings);
            
                var noSqlEntity = InterestRateSettingsNoSqlEntity.Create(settings);
                await _interestRateWriter.InsertOrReplaceAsync(noSqlEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task RemoveSettings(InterestRateSettings settings)
        {
            try
            {
                await using var ctx = _contextFactory.Create();
                await ctx.RemoveSettings(settings);
                
                await _interestRateWriter.DeleteAsync(InterestRateSettingsNoSqlEntity.GeneratePartitionKey(settings.WalletId, settings.Asset),
                    InterestRateSettingsNoSqlEntity.GenerateRowKey(settings.RangeFrom, settings.RangeTo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}