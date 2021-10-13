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
            
            await UpdateIndexPrices(ctx);
            await ctx.ExecCalculationAsync(calculationDate, _logger);
        }

        private async Task UpdateIndexPrices(DatabaseContext databaseContext)
        {
            var indexPrices = _indexPricesClient.GetIndexPricesAsync();
            if (!indexPrices.Any())
            {
                await Task.Delay(5000);
                indexPrices = _indexPricesClient.GetIndexPricesAsync();
            }
            if (!indexPrices.Any())
            {
                await Task.Delay(5000);
                indexPrices = _indexPricesClient.GetIndexPricesAsync();
            }
            if (indexPrices.Any())
            {
                var localIndexPrices = indexPrices.Select(e => new IndexPriceEntity()
                {
                    Asset = e.Asset,
                    PriceInUsd = e.UsdPrice
                });
                await databaseContext.UpdateIndexPrice(localIndexPrices);
            }
        }
    }
}