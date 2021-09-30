using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Services
{
    public class InterestRateSettingsService: IInterestRateSettingsService
    {
        private readonly ILogger<InterestRateSettingsService> _logger;
        private readonly IInterestRateSettingsStorage _interestRateSettingsStorage;

        public InterestRateSettingsService(ILogger<InterestRateSettingsService> logger, 
            IInterestRateSettingsStorage interestRateSettingsStorage)
        {
            _logger = logger;
            _interestRateSettingsStorage = interestRateSettingsStorage;
        }

        public async Task<GetInterestRateSettingsResponse> GetInterestRateSettingsAsync()
        {
            try
            {
                var rates = await _interestRateSettingsStorage.GetSettings();
                return new GetInterestRateSettingsResponse()
                {
                    Success = true,
                    InterestRateCollection = rates
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
                await _interestRateSettingsStorage.UpsertSettings(request.InterestRateSettings);
                
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
                await _interestRateSettingsStorage.RemoveSettings(request.InterestRateSettings);

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
