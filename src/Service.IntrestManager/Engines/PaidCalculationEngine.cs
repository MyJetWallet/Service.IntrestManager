using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.IndexPrices.Client;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Storage;

namespace Service.IntrestManager.Engines
{
    public class PaidCalculationEngine
    {
        private readonly ILogger<PaidCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IIndexPricesClient _indexPricesClient;

        public PaidCalculationEngine(ILogger<PaidCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory, 
            IIndexPricesClient indexPricesClient)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _indexPricesClient = indexPricesClient;
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
            var calculationsForWeek =
                ctx.GetInterestRateCalculationByDateRange(dateFrom, dateTo);
            var paidCollection = new List<InterestRatePaid>();
            var createdDate = InterestConstants.PaidCreatedDate;
            
            var wallets = calculationsForWeek.Select(e => e.WalletId).Distinct().ToList();
            foreach (var walletId in wallets)
            {
                var calculationsByWallet = calculationsForWeek.Where(e => e.WalletId == walletId).ToList();
                foreach (var symbol in calculationsByWallet.Select(e => e.Symbol).Distinct())
                {
                    var calculationsByWalletAndSymbol = calculationsByWallet.Where(e => e.Symbol == symbol);

                    var amount = calculationsByWalletAndSymbol.Sum(e => e.Amount);
                    if (amount == 0)
                        continue;
                    
                    paidCollection.Add(new InterestRatePaid()
                    {
                        TransactionId = Guid.NewGuid().ToString("N"),
                        WalletId = walletId,
                        Date = createdDate,
                        Symbol = symbol,
                        Amount = amount,
                        State = PaidState.New
                    });
                }
            }
            await ctx.SavePaidCollection(paidCollection);

            await SavePaidHistory(ctx, wallets.Count, paidCollection, dateFrom, dateTo, createdDate);
            
            _logger.LogInformation($"CalculatePaid finish work with dateFrom: {dateFrom} and dateTo: {dateTo}. Saved {paidCollection.Count} paid records.");
        }

        private async Task SavePaidHistory(DatabaseContext ctx, int walletsCount,
            IReadOnlyCollection<InterestRatePaid> paidCollection, 
            DateTime rangeFrom, DateTime rangeTo, DateTime createdDate)
        {
            var totalAmount = 0m;
            foreach (var symbol in paidCollection.Select(e => e.Symbol).Distinct())
            {
                var sumBySymbol = paidCollection.Where(e => e.Symbol == symbol).Sum(e => e.Amount);
                var (_, usdVolume) = _indexPricesClient.GetIndexPriceByAssetVolumeAsync(symbol, sumBySymbol);
                totalAmount += usdVolume;
            }
            var paidHistory = new PaidHistory()
            {
                CreatedDate = createdDate,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                WalletCount = walletsCount,
                TotalPaidInUsd = totalAmount
            };
            await ctx.SavePaidHistory(paidHistory);
            _logger.LogInformation("Saved paid history: {historyJson}.", JsonConvert.SerializeObject(paidHistory));
        }
    }
}