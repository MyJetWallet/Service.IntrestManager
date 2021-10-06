using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.IntrestManager.Engines;

namespace Service.IntrestManager.Jobs
{
    public class InterestManagerJob : IStartable
    {
        private readonly ILogger<InterestManagerJob> _logger;
        private readonly MyTaskTimer _timer;

        private readonly InterestCalculationEngine _interestCalculationEngine;
        private readonly PaidCalculationEngine _paidCalculationEngine;
        private readonly InterestProcessingEngine _interestProcessingEngine;

        public InterestManagerJob(ILogger<InterestManagerJob> logger, 
            InterestCalculationEngine interestCalculationEngine, 
            PaidCalculationEngine paidCalculationEngine, 
            InterestProcessingEngine interestProcessingEngine)
        {
            _logger = logger;
            _interestCalculationEngine = interestCalculationEngine;
            _paidCalculationEngine = paidCalculationEngine;
            _interestProcessingEngine = interestProcessingEngine;

            _timer = new MyTaskTimer(nameof(InterestManagerJob), 
                TimeSpan.FromSeconds(Program.Settings.InterestManagerTimerInSeconds), _logger, DoTime);
            _logger.LogInformation($"InterestManagerJob timer: {TimeSpan.FromSeconds(Program.Settings.InterestManagerTimerInSeconds)}");
        }

        private async Task DoTime()
        {
            await _interestCalculationEngine.Execute();
            await _paidCalculationEngine.Execute();
            await _interestProcessingEngine.Execute();
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}