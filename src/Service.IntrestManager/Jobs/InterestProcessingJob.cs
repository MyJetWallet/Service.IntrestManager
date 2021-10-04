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
    public class InterestProcessingJob : IStartable
    {
        private readonly ILogger<InterestProcessingJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public InterestProcessingJob(ILogger<InterestProcessingJob> logger, DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            
            _timer = new MyTaskTimer(nameof(InterestProcessingJob), 
                TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds), _logger, DoTime);
            _logger.LogInformation($"InterestProcessingJob timer: {TimeSpan.FromSeconds(Program.Settings.InterestProcessingTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            await ProcessInterest();
        }

        private async Task ProcessInterest()
        {
            await using var ctx = _databaseContextFactory.Create();
            var paidToProcess = ctx.GetNewPaidCollection();

            while (paidToProcess.Any())
            {
                _logger.LogInformation($"InterestProcessingJob find {paidToProcess.Count} new records to process at {DateTime.UtcNow}.");
                foreach (var interestRatePaid in paidToProcess)
                {
                    // await Task.Delay(500);
                    // todo: process paid and process errors
                    interestRatePaid.State = PaidState.Completed;
                }
                await ctx.SaveChangesAsync();
                _logger.LogInformation($"InterestProcessingJob finish process {paidToProcess.Count} records.");

                paidToProcess = ctx.GetNewPaidCollection() ?? new List<InterestRatePaid>();
            }
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}