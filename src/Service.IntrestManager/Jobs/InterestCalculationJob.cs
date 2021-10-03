using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.InterestManager.Postrges;

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
            await using var ctx = _databaseContextFactory.Create();
            await ctx.ExecCalculationAsync(DateTime.UtcNow);
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}