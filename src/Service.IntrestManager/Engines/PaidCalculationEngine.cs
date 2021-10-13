using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Engines
{
    public class PaidCalculationEngine
    {
        private readonly ILogger<PaidCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IndexPriceEngine _indexPriceEngine;

        public PaidCalculationEngine(ILogger<PaidCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory,
            IndexPriceEngine indexPriceEngine)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _indexPriceEngine = indexPriceEngine;
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
            var lastPaid = ctx.GetLastPaid();

            if (lastPaid == null)
            {
                return true;
            }
            var paidExpected = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday &&
                               lastPaid.CreatedDate.Date != DateTime.UtcNow.Date;
            return paidExpected;
        }

        private async Task CalculatePaid()
        {
            await using var ctx = _databaseContextFactory.Create();
            
            var dateFrom = ctx.GetLastPaid()?.RangeTo.AddMilliseconds(1) ?? DateTime.MinValue;
            var dateTo = InterestConstants.PaidPeriodToDate;
            
            _logger.LogInformation($"CalculatePaid started work with dateFrom: {dateFrom} and dateTo: {dateTo}");
            
            await _indexPriceEngine.UpdateIndexPrices(ctx);
            await ctx.ExecPaidAsync(dateFrom, dateTo, _logger);
        }
    }
}