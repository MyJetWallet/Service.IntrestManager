using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Engines
{
    public class InterestCalculationEngine
    {
        private readonly ILogger<InterestCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IndexPriceEngine _indexPriceEngine;
        private readonly IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> _ratesWriter;

        public InterestCalculationEngine(ILogger<InterestCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory, 
            IndexPriceEngine indexPriceEngine, 
            IMyNoSqlServerDataWriter<InterestRateByWalletNoSql> ratesWriter)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _indexPriceEngine = indexPriceEngine;
            _ratesWriter = ratesWriter;
        }

        public async Task Execute()
        {
            using var activity = MyTelemetry.StartActivity(nameof(InterestCalculationEngine));
            if (await CalculationExpected())
            {
                await CalculateInterest();
            }
        }

        private async Task<bool> CalculationExpected()
        {
            await using var ctx = _databaseContextFactory.Create();
            var lastCalculation = ctx.GetLastCalculationHistory();

            if (lastCalculation == null)
            {
                return true;
            }
            var calculationExpected = lastCalculation.CompletedDate.Date != DateTime.UtcNow.Date;
            return calculationExpected;
        }

        private async Task CalculateInterest()
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.Database.SetCommandTimeout(1200);

            var calculationDate = InterestConstants.CalculationPeriodDate;
            
            await _indexPriceEngine.UpdateIndexPrices(ctx);
            await ctx.ExecCalculationAsync(calculationDate, _logger);
            await ctx.ExecCurrentCalculationAsync(calculationDate, _logger);

            await _ratesWriter.CleanAndKeepMaxPartitions(0);
        }
    }
}