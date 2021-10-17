using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Grpc;

namespace Service.IntrestManager.Api.Services
{
    public class InterestRateClientService : IInterestRateClientService
    {
        private readonly ILogger<InterestRateClientService> _logger;
        private readonly IInterestRateByWalletStorage _interestRateByWalletStorage;
        

        public InterestRateClientService(ILogger<InterestRateClientService> logger, 
            IInterestRateByWalletStorage interestRateByWalletStorage)
        {
            _logger = logger;
            _interestRateByWalletStorage = interestRateByWalletStorage;
        }

        public async Task<GetInterestRatesByWalletResponse> GetInterestRatesByWalletAsync(GetInterestRatesByWalletRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.WalletId))
                {
                    return new GetInterestRatesByWalletResponse()
                    {
                        Success = false,
                        ErrorMessage = "WalletId is empty."
                    };
                }
                var rates = await _interestRateByWalletStorage.GetRatesByWallet(request.WalletId);
                return new GetInterestRatesByWalletResponse()
                {
                    Success = true,
                    Rates = rates
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetInterestRatesByWalletResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}