using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.IndexPrices.Client;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Engines
{
    public class InterestCalculationEngine
    {
        private readonly ILogger<InterestCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IIndexPricesClient _indexPricesClient;

        public InterestCalculationEngine(ILogger<InterestCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory, 
            IIndexPricesClient indexPricesClient)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _indexPricesClient = indexPricesClient;
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
            var lastCalculation = ctx.GetLastCalculation();

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
            await ctx.ExecCalculationAsync(calculationDate, _logger);

            await SaveCalculationHistory(ctx, calculationDate, InterestConstants.CalculationExecutedDate);
        }

        private async Task SaveCalculationHistory(DatabaseContext ctx, DateTime calculationDate, DateTime completedDate)
        {
            var calculations = ctx.GetInterestRateCalculationByDate(calculationDate);
            var walletCount = calculations.Select(e => e.WalletId).Distinct().Count();

            var amountInWalletsInUsd = 0m;
            var calculatedAmountInUsd = 0m;
            foreach (var symbol in calculations.Select(e => e.Symbol).Distinct())
            {
                var calculationsBySymbol = calculations.Where(e => e.Symbol == symbol);
                
                var amountSum = calculationsBySymbol.Sum(e => e.NewBalance);
                var (_, usdAmountVolume) = _indexPricesClient.GetIndexPriceByAssetVolumeAsync(symbol, amountSum);
                amountInWalletsInUsd += usdAmountVolume;
                
                var apySum = calculationsBySymbol.Sum(e => e.Amount);
                var (_, usdApyVolume) = _indexPricesClient.GetIndexPriceByAssetVolumeAsync(symbol, apySum);
                calculatedAmountInUsd += usdApyVolume;
            }
            
            var calculationHistory = new CalculationHistory()
            {
                CalculationDate = calculationDate,
                CompletedDate = completedDate,
                WalletCount = walletCount,
                AmountInWalletsInUsd = amountInWalletsInUsd,
                CalculatedAmountInUsd = calculatedAmountInUsd
            };
            await ctx.SaveCalculationHistory(calculationHistory);
            _logger.LogInformation("Saved calculation history: {historyJson}.", JsonConvert.SerializeObject(calculationHistory));
        }
    }
}