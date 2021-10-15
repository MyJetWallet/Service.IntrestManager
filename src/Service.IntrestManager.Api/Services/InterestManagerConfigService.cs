using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Api.Services
{
    public class InterestManagerConfigService : IInterestManagerConfigService
    {
        private readonly ILogger<InterestManagerConfigService> _logger;
        private readonly IMyNoSqlServerDataWriter<InterestManagerConfigNoSql> _configWriter;

        public InterestManagerConfigService(ILogger<InterestManagerConfigService> logger, 
            IMyNoSqlServerDataWriter<InterestManagerConfigNoSql> configWriter)
        {
            _logger = logger;
            _configWriter = configWriter;
        }

        public async Task<GetInterestManagerConfigResponse> GetInterestManagerConfigAsync()
        {
            try
            {
                var configNoSql = await _configWriter.GetAsync();
                return new GetInterestManagerConfigResponse()
                {
                    Success = true,
                    Config = configNoSql.FirstOrDefault()?.Config
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetInterestManagerConfigResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UpsertInterestManagerConfigResponse> UpsertInterestManagerConfigAsync(UpsertInterestManagerConfigRequest request)
        {
            try
            {
                await _configWriter.InsertOrReplaceAsync(InterestManagerConfigNoSql.Create(request.Config));
                
                return new UpsertInterestManagerConfigResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new UpsertInterestManagerConfigResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}