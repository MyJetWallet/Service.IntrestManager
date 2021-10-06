using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Services
{
    public class InterestManagerService : IInterestManagerService
    {
        private readonly ILogger<InterestManagerService> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;

        public InterestManagerService(ILogger<InterestManagerService> logger, 
            DatabaseContextFactory databaseContextFactory)
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task<GetCalculationHistoryResponse> GetCalculationHistoryAsync()
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var calcCollection = ctx.GetCalculationHistory();
                return new GetCalculationHistoryResponse()
                {
                    Success = true,
                    History = calcCollection
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetCalculationHistoryResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetPaidHistoryResponse> GetPaidHistoryAsync()
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var paidCollection = ctx.GetPaidHistory();
                return new GetPaidHistoryResponse()
                {
                    Success = true,
                    History = paidCollection
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetPaidHistoryResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}