using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;

namespace Service.IntrestManager.Jobs
{
    public class InterestCalculationJob : IStartable
    {
        private readonly ILogger<InterestCalculationJob> _logger;
        private readonly MyTaskTimer _timer;

        public InterestCalculationJob(ILogger<InterestCalculationJob> logger)
        {
            _logger = logger;
            
            _timer = new MyTaskTimer(nameof(InterestCalculationJob), 
                TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds), _logger, DoTime);
            _logger.LogInformation($"InterestCalculationJob timer: {TimeSpan.FromSeconds(Program.Settings.InterestCalculationTimerInSeconds)}");
        }

        private async Task DoTime()
        {
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}