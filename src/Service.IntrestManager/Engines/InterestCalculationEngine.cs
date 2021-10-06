using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Engines
{
    public class InterestCalculationEngine
    {
        private readonly ILogger<InterestCalculationEngine> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public InterestCalculationEngine(ILogger<InterestCalculationEngine> logger, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task Execute()
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
            await ctx.ExecCalculationAsync(DateTime.UtcNow, _logger);

            await SaveCalculationHistory(ctx);
        }

        private async Task SaveCalculationHistory(DatabaseContext ctx)
        {
            var calculationHistory = new CalculationHistory()
            {
                CompletedDate = DateTime.UtcNow
            };
            await ctx.SaveCalculationHistory(calculationHistory);
            _logger.LogInformation("Saved calculation history: {historyJson}.", JsonConvert.SerializeObject(calculationHistory));
        }
    }
}