using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Services
{
    public class InterestRateSettingsService: IInterestRateSettingsService
    {
        private readonly ILogger<InterestRateSettingsService> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestRateNoSqlEntity> _interestRateWriter;

        public InterestRateSettingsService(ILogger<InterestRateSettingsService> logger, 
            IMyNoSqlServerDataWriter<InterestRateNoSqlEntity> interestRateWriter)
        {
            _logger = logger;
            _interestRateWriter = interestRateWriter;
        }

        public async Task<GetInterestRateSettingsResponse> GetInterestRateSettingsAsync()
        {
            try
            {
                var rates = await _interestRateWriter.GetAsync();
                return new GetInterestRateSettingsResponse()
                {
                    Success = true,
                    InterestRateCollection = rates.Select(e => e.InterestRate).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetInterestRateSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UpsertInterestRateSettingsResponse> UpsertInterestRateSettingsAsync(UpsertInterestRateSettingsRequest request)
        {
            try
            {
                var noSqlEntity = InterestRateNoSqlEntity.Create(request.InterestRate);
                await _interestRateWriter.InsertOrReplaceAsync(noSqlEntity);
                
                return new UpsertInterestRateSettingsResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new UpsertInterestRateSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RemoveInterestRateSettingsResponse> RemoveInterestRateSettingsAsync(RemoveInterestRateSettingsRequest request)
        {
            try
            {
                await _interestRateWriter.DeleteAsync(InterestRateNoSqlEntity.GeneratePartitionKey(request.InterestRate.WalletId, request.InterestRate.Asset),
                    InterestRateNoSqlEntity.GenerateRowKey(request.InterestRate.RangeFrom, request.InterestRate.RangeTo));

                return new RemoveInterestRateSettingsResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new RemoveInterestRateSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
