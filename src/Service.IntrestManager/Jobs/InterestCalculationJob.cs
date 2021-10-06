using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Jobs
{
    public class InterestCalculationJob : IStartable
    {
        private readonly ILogger<InterestCalculationJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public InterestCalculationJob(ILogger<InterestCalculationJob> logger, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;

            _timer = new MyTaskTimer(nameof(InterestCalculationJob), 
                TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds), _logger, DoTime);
            _logger.LogInformation($"InterestCalculationJob timer: {TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            if (await CalculationExpected())
            {
                await CalculateInterest();
            }
        }

        private async Task<bool> CalculationExpected()
        {
            await using var ctx = _databaseContextFactory.Create();
            var lastCalculation = ctx.GetLastCalculation();

            var calculationExpected = lastCalculation.Date.Date != DateTime.UtcNow.Date;
            return calculationExpected;
        }

        private async Task CalculateInterest()
        {
            await using var ctx = _databaseContextFactory.Create();
            ctx.Database.SetCommandTimeout(1200);
            await ctx.ExecCalculationAsync(DateTime.UtcNow, _logger);
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}