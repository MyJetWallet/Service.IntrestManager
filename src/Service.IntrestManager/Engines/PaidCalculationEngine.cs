using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Engines
{
    public class PaidCalculationEngine
    {
        private readonly ILogger<PaidCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IndexPriceEngine _indexPriceEngine;
        private readonly IInterestManagerConfigService _interestManagerConfigService;

        public PaidCalculationEngine(ILogger<PaidCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory,
            IndexPriceEngine indexPriceEngine, 
            IInterestManagerConfigService interestManagerConfigService)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _indexPriceEngine = indexPriceEngine;
            _interestManagerConfigService = interestManagerConfigService;
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
            var serviceConfig = await _interestManagerConfigService.GetInterestManagerConfigAsync();
            return serviceConfig.Config.PaidPeriod switch
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
            
            var dateFrom = ctx.GetLastPaid()?.RangeTo.AddMilliseconds(1) ?? DateTime.MinValue;
            var dateTo = InterestConstants.PaidPeriodToDate;
            
            _logger.LogInformation($"CalculatePaid started work with dateFrom: {dateFrom} and dateTo: {dateTo}");
            
            await _indexPriceEngine.UpdateIndexPrices(ctx);
            await ctx.ExecPaidAsync(dateFrom, dateTo, _logger);
        }
    }
}