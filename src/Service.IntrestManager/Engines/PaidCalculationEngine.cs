using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Engines
{
    public class PaidCalculationEngine
    {
        private readonly ILogger<PaidCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IndexPriceEngine _indexPriceEngine;
        private readonly IMyNoSqlServerDataReader<InterestManagerConfigNoSql> _myNoSqlServerDataReader;

        public PaidCalculationEngine(ILogger<PaidCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory,
            IndexPriceEngine indexPriceEngine,
            IMyNoSqlServerDataReader<InterestManagerConfigNoSql> myNoSqlServerDataReader)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _indexPriceEngine = indexPriceEngine;
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
        }

        public async Task Execute()
        {
            using var activity = MyTelemetry.StartActivity(nameof(PaidCalculationEngine));
            if (await GedPaidExpectedState())
            {
                await CalculatePaid();
            }
        }

        private async Task<bool> GedPaidExpectedState()
        {
            await using var ctx = _databaseContextFactory.Create();
            var lastPaid = ctx.GetLastPaidHistory();

            if (lastPaid == null)
            {
                return true;
            }
            var serviceConfig = _myNoSqlServerDataReader.Get().FirstOrDefault();
            return serviceConfig?.Config.PaidPeriod switch
            {
                PaidPeriod.Day => lastPaid.CreatedDate.Date != DateTime.UtcNow.Date,
                PaidPeriod.Week => DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday &&
                                   lastPaid.CreatedDate.Date != DateTime.UtcNow.Date,
                _ => DateTime.UtcNow.Day == 1 && lastPaid.CreatedDate.Date != DateTime.UtcNow.Date
            };
        }

        private async Task CalculatePaid()
        {
            await using var ctx = _databaseContextFactory.Create();
            
            var dateFrom = ctx.GetLastPaidHistory()?.RangeTo.AddMilliseconds(1) ?? DateTime.MinValue;
            var dateTo = InterestConstants.PaidPeriodToDate;
            
            _logger.LogInformation($"CalculatePaid started work with dateFrom: {dateFrom} and dateTo: {dateTo}");
            
            await _indexPriceEngine.UpdateIndexPrices(ctx);
            await ctx.ExecPaidAsync(dateFrom, dateTo, _logger);

            await ctx.ExecTotalCalculationAsync(_logger);
            await ctx.ExecCurrentCalculationAsync(dateTo, _logger);
        }
    }
}