using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.IntrestManager.Domain;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Api.Services
{
    public class InterestRateSettingsService : IInterestRateSettingsService
    {
        private readonly ILogger<InterestRateSettingsService> _logger;
        private readonly IInterestRateSettingsStorage _interestRateSettingsStorage;
        private readonly IMyNoSqlServerDataWriter<InterestManagerConfigNoSql> _myNoSqlServerDataWriter;

        public InterestRateSettingsService(ILogger<InterestRateSettingsService> logger,
            IInterestRateSettingsStorage interestRateSettingsStorage, 
            IMyNoSqlServerDataWriter<InterestManagerConfigNoSql> myNoSqlServerDataWriter)
        {
            _logger = logger;
            _interestRateSettingsStorage = interestRateSettingsStorage;
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
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

        public async Task<UpsertInterestRateSettingsResponse> UpsertInterestRateSettingsAsync(
            UpsertInterestRateSettingsRequest request)
        {
            try
            {
                RecalculateApy(new List<InterestRateSettings> { request.InterestRateSettings },
                    (await _myNoSqlServerDataWriter.GetAsync()).FirstOrDefault()?.Config?.PaidPeriod ?? PaidPeriod.Day);
                var result = await _interestRateSettingsStorage.UpsertSettings(request.InterestRateSettings);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    return new UpsertInterestRateSettingsResponse()
                    {
                        Success = false,
                        ErrorMessage = result
                    };
                }

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

        public async Task<UpsertInterestRateSettingsListResponse> UpsertInterestRateSettingsListAsync(
            UpsertInterestRateSettingsListRequest request)
        {
            try
            {
                RecalculateApy(request.InterestRateSettings,
                    (await _myNoSqlServerDataWriter.GetAsync()).FirstOrDefault()?.Config?.PaidPeriod ?? PaidPeriod.Day);
                var validationResult =
                    await _interestRateSettingsStorage.UpsertSettingsList(request.InterestRateSettings);

                return new UpsertInterestRateSettingsListResponse()
                {
                    Success = validationResult.All(e => e.ValidationResult == SettingsValidationResultEnum.Ok),
                    ValidationResult = validationResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new UpsertInterestRateSettingsListResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RemoveInterestRateSettingsResponse> RemoveInterestRateSettingsAsync(
            RemoveInterestRateSettingsRequest request)
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

        public async Task<SyncDbAndNoSqlResponse> SyncDbAndNoSqlAsync()
        {
            try
            {
                await _interestRateSettingsStorage.SyncSettings();

                return new SyncDbAndNoSqlResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new SyncDbAndNoSqlResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private void RecalculateApy(List<InterestRateSettings> settings, PaidPeriod paidPeriod)
        {
            double timesAppliedPerYear = 365;
            switch (paidPeriod)
            {
                case PaidPeriod.Day:
                    timesAppliedPerYear = 365;
                    break;
                case PaidPeriod.Week:
                    timesAppliedPerYear = 365.0 / 7;
                    break;
                case PaidPeriod.Month:
                    timesAppliedPerYear = 365.0 / 30;
                    break;
            }

            foreach (var interestRateSettings in settings)
            {
                interestRateSettings.Apy = interestRateSettings.Apr == 0
                    ? 0
                    : decimal.Round(
                        Convert.ToDecimal(100 *
                                          (Math.Pow(
                                               (1 + decimal.ToDouble(interestRateSettings.Apr) / 100 /
                                                   timesAppliedPerYear), timesAppliedPerYear) -
                                           1)), 2, MidpointRounding.ToZero);
            }
        }
    }
}