using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Grpc;
using Service.IntrestManager.Grpc.Models;

namespace Service.IntrestManager.Api.Services
{
    public class InterestManagerService : IInterestManagerService
    {
        private readonly ILogger<InterestManagerService> _logger;
        private readonly DatabaseContextFactory _databaseContextFactory;
        private readonly IMyNoSqlServerDataReader<InterestManagerConfigNoSql> _myNoSqlServerDataReader;

        public InterestManagerService(ILogger<InterestManagerService> logger, 
            DatabaseContextFactory databaseContextFactory,
            IMyNoSqlServerDataReader<InterestManagerConfigNoSql> myNoSqlServerDataReader
            )
        {
            _logger = logger;
            _databaseContextFactory = databaseContextFactory;
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
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
                    request.BatchSize, request.AssetFilter, request.WalletFilter, 
                    request.DateFilter, request.HistoryFilter);
                
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
                    request.BatchSize, request.AssetFilter, request.WalletFilter, 
                    request.DateFilter, request.StateFilter, request.HistoryFilter);
                
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
        
        public async Task<GetPaidExpectedDateResponse> GetPaidExpectedDateAsync(GetPaidExpectedDateResponse request)
        {
            try
            {
                var expectedDate = await GetPaidExpectedDateAsync();
                var serviceConfig = _myNoSqlServerDataReader.Get().FirstOrDefault();

                return new GetPaidExpectedDateResponse
                {
                    ExpectedDate = expectedDate,
                    Period = serviceConfig.Config.PaidPeriod
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to GetPaidExpectedDate");
                return new GetPaidExpectedDateResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<DateTime> GetPaidExpectedDateAsync()
        {
            var serviceConfig = _myNoSqlServerDataReader.Get().FirstOrDefault();
            await using var ctx = _databaseContextFactory.Create();
            var lastPaid = ctx.GetLastPaidHistory();
            
            if (lastPaid == null)
            {
                return DateTime.UtcNow;
            }

            switch (serviceConfig?.Config?.PaidPeriod)
            {
                case PaidPeriod.Day:
                {
                    return lastPaid?.CreatedDate.Day == DateTime.UtcNow.Day
                        ? DateTime.UtcNow.Date.AddDays(1)
                        : DateTime.UtcNow.Date;
                }
                case PaidPeriod.Week:
                {
                    switch (DateTime.UtcNow.DayOfWeek)
                    {
                        case DayOfWeek.Monday when lastPaid?.CreatedDate.Day == DateTime.UtcNow.Day:
                        {
                            var today = DateTime.Today;
                            var nextMonday = Enumerable.Range(0, 6)
                                .Select(i => today.AddDays(i))
                                .Single(day => day.DayOfWeek == DayOfWeek.Monday);
                            return nextMonday;
                        }
                        case DayOfWeek.Monday:
                            return DateTime.UtcNow.Date;
                        default:
                        {
                            var today = DateTime.Today;
                            var nextMonday = Enumerable.Range(0, 6)
                                .Select(i => today.AddDays(i))
                                .Single(day => day.DayOfWeek == DayOfWeek.Monday);
                            return nextMonday;
                        }
                    }
                }
                case PaidPeriod.Month:
                {
                    if (lastPaid?.CreatedDate.Month == DateTime.UtcNow.Month)
                    {
                        var nextMonth = DateTime.UtcNow.Month == 12 ? 1 : DateTime.UtcNow.Month + 1;
                        var nextYear = DateTime.UtcNow.Month == 12 ? DateTime.UtcNow.Year + 1 : DateTime.UtcNow.Year;
                        return new DateTime(nextYear, nextMonth, 1);
                    }

                    return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                }
                default: throw new NotSupportedException($"Period {serviceConfig?.Config?.PaidPeriod}");
            }
        }
    }
}