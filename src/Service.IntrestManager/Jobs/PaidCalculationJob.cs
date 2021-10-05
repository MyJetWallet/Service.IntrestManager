using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Jobs
{
    public class PaidCalculationJob : IStartable
    {
        private readonly ILogger<PaidCalculationJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public PaidCalculationJob(ILogger<PaidCalculationJob> logger, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            
            _timer = new MyTaskTimer(nameof(PaidCalculationJob), 
                TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds), _logger, DoTime);
            _logger.LogInformation($"PaidCalculationJob timer: {TimeSpan.FromSeconds(Program.Settings.PaidCalculationTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            if (await GedPaidExpectedState())
            {
                await CalculatePaid();
            }
        }

        private async Task<bool> GedPaidExpectedState()
        {
            await using var ctx = _databaseContextFactory.Create();
            var lastPaid = ctx.GetLastPaid();

            var paidExpected = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday &&
                               lastPaid.Date.Date != DateTime.UtcNow.Date;
            return paidExpected;
        }

        private async Task CalculatePaid()
        {
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var dateTo = DateTime.UtcNow;
            
            _logger.LogInformation($"CalculatePaid started work with dateFrom: {dateFrom} and dateTo: {dateTo}");
            await using var ctx = _databaseContextFactory.Create();
            var calculationsForWeek =
                ctx.GetInterestRateCalculationByDate(dateFrom, dateTo);
            var paidCollection = new List<InterestRatePaid>();
            var currentDate = DateTime.UtcNow;
            foreach (var walletId in calculationsForWeek.Select(e => e.WalletId).Distinct())
            {
                var calculationsByWallet = calculationsForWeek.Where(e => e.WalletId == walletId).ToList();
                foreach (var symbol in calculationsByWallet.Select(e => e.Symbol).Distinct())
                {
                    var calculationsByWalletAndSymbol = calculationsByWallet.Where(e => e.Symbol == symbol);
                    paidCollection.Add(new InterestRatePaid()
                    {
                        WalletId = walletId,
                        Date = currentDate,
                        Symbol = symbol,
                        Amount = calculationsByWalletAndSymbol.Sum(e => e.Amount),
                        State = PaidState.New
                    });
                }
            }
            await ctx.SavePaidCollection(paidCollection);
            _logger.LogInformation($"CalculatePaid finish work with dateFrom: {dateFrom} and dateTo: {dateTo}. Saved {paidCollection.Count} paid records.");
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}