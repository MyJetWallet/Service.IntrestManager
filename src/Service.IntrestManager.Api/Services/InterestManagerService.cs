using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Api.Services
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

        public async Task<GetCalculationsResponse> GetCalculationsAsync(GetCalculationsRequest request)
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var calculations = ctx.GetCalculationByFilter(request.LastId,
                    request.BatchSize, request.AssetFilter, request.WalletFilter, request.DateFilter);
                
                long idForNextQuery = 0;
                if (calculations.Any())
                {
                    idForNextQuery = calculations.Select(elem => elem.Id).Min();
                }
                return new GetCalculationsResponse()
                {
                    Success = true,
                    Calculations = calculations,
                    IdForNextQuery = idForNextQuery
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetCalculationsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetPaidResponse> GetPaidAsync(GetPaidRequest request)
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                var paid = ctx.GetPaidByFilter(request.LastId,
                    request.BatchSize, request.AssetFilter, request.WalletFilter, request.DateFilter, request.StateFilter);
                
                long idForNextQuery = 0;
                if (paid.Any())
                {
                    idForNextQuery = paid.Select(elem => elem.Id).Min();
                }
                return new GetPaidResponse()
                {
                    Success = true,
                    Paid = paid,
                    IdForNextQuery = idForNextQuery
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetPaidResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RetryPaidByCreatedDateResponse> RetryPaidByCreatedDateAsync(RetryPaidByCreatedDateRequest request)
        {
            try
            {
                await using var ctx = _databaseContextFactory.Create();
                await ctx.RetryPaidPeriod(request.CreatedDate);

                return new RetryPaidByCreatedDateResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new RetryPaidByCreatedDateResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}