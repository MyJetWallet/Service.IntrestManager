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

        public InterestRateSettingsStorage(IMyNoSqlServerDataWriter<InterestRateSettingsNoSql> interestRateWriter,
            DatabaseContextFactory contextFactory, 
            ILogger<InterestRateSettingsStorage> logger)
        {
            _interestRateWriter = interestRateWriter;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        private async Task SyncSettings()
        {
            await using var ctx = _contextFactory.Create();
            var settings = ctx.GetSettings();

            var noSqlSettings = settings.Select(InterestRateSettingsNoSql.Create).ToList();

            await _interestRateWriter.CleanAndKeepMaxPartitions(0);
            await _interestRateWriter.BulkInsertOrReplaceAsync(noSqlSettings);
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

        public async Task UpsertSettings(InterestRateSettings settings)
        {
            try
            {
                await using var ctx = _contextFactory.Create();
                await ctx.UpsertSettings(settings);

                await SyncSettings();
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

                await SyncSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}