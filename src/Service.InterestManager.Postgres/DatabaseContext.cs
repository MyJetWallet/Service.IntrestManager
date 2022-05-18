using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Postgres;
using Service.IntrestManager.Domain.Models;

namespace Service.InterestManager.Postrges
{
    public class DatabaseContext : MyDbContext
    {
        public const string Schema = "interest_manager";
        
        private const string InterestRateSettingsTableName = "interestratesettings";
        private const string InterestRateCalculationTableName = "interestratecalculation";
        private const string InterestRatePaidTableName = "interestratepaid";
        private const string CalculationHistoryTableName = "calculationhistory";
        private const string PaidHistoryTableName = "paidhistory";
        private const string IndexPriceTableName = "indexprice";
        private const string InterestRateStateTableName = "interestratestate";
        
        private DbSet<InterestRateSettings> InterestRateSettingsCollection { get; set; }
        public DbSet<InterestRateCalculation> InterestRateCalculationCollection { get; set; }
        private DbSet<InterestRatePaid> InterestRatePaidCollection { get; set; }
        public DbSet<CalculationHistory> CalculationHistoryCollection { get; set; }
        private DbSet<PaidHistory> PaidHistoryCollection { get; set; }
        private DbSet<IndexPriceEntity> IndexPriceCollection { get; set; }
        private DbSet<InterestRateState> InterestRateStates { get; set; }
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetInterestRateSettingsEntity(modelBuilder);
            SetInterestRateCalculationEntity(modelBuilder);
            SetInterestRatePaidEntity(modelBuilder);
            SetCalculationHistoryEntity(modelBuilder);
            SetPaidHistoryEntity(modelBuilder);
            SetIndexPriceEntity(modelBuilder);
            SetInterestRateStatesEntity(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SetIndexPriceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexPriceEntity>().ToTable(IndexPriceTableName);
            
            modelBuilder.Entity<IndexPriceEntity>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<IndexPriceEntity>().HasKey(e => e.Id);
            
            modelBuilder.Entity<IndexPriceEntity>().Property(e => e.Asset).HasMaxLength(64);
            modelBuilder.Entity<IndexPriceEntity>().Property(e => e.PriceInUsd);
            
            modelBuilder.Entity<IndexPriceEntity>().HasIndex(e => e.Asset);
        }

        private void SetCalculationHistoryEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculationHistory>().ToTable(CalculationHistoryTableName);
            
            modelBuilder.Entity<CalculationHistory>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<CalculationHistory>().HasKey(e => e.Id);
            
            modelBuilder.Entity<CalculationHistory>().Property(e => e.CalculationDate);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.CompletedDate);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.WalletCount);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.AmountInWalletsInUsd);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.CalculatedAmountInUsd);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.SettingsJson);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.LastTs).HasDefaultValue(DateTime.MinValue);

            modelBuilder.Entity<CalculationHistory>().HasIndex(e => e.CompletedDate);

            modelBuilder.Entity<CalculationHistory>().HasIndex(e => e.LastTs);
        }

        private void SetPaidHistoryEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaidHistory>().ToTable(PaidHistoryTableName);
            
            modelBuilder.Entity<PaidHistory>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<PaidHistory>().HasKey(e => e.Id);
            
            modelBuilder.Entity<PaidHistory>().Property(e => e.CreatedDate);
            modelBuilder.Entity<PaidHistory>().Property(e => e.RangeFrom);
            modelBuilder.Entity<PaidHistory>().Property(e => e.RangeTo);
            modelBuilder.Entity<PaidHistory>().Property(e => e.WalletCount);
            modelBuilder.Entity<PaidHistory>().Property(e => e.TotalPaidInUsd);
            modelBuilder.Entity<PaidHistory>().Property(e => e.LastTs).HasDefaultValue(DateTime.MinValue);

            modelBuilder.Entity<PaidHistory>().HasIndex(e => e.CreatedDate);

            modelBuilder.Entity<PaidHistory>().HasIndex(e => e.LastTs);
        }

        private void SetInterestRatePaidEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InterestRatePaid>().ToTable(InterestRatePaidTableName);
            
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<InterestRatePaid>().HasKey(e => e.Id);
            
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.WalletId).HasMaxLength(64);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.Symbol).HasMaxLength(64);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.Date);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.Amount);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.State);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.ErrorMessage);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.TransactionId);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.HistoryId);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.Iteration);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.DatePaid).HasDefaultValue(DateTime.MinValue);
            modelBuilder.Entity<InterestRatePaid>().Property(e => e.LastTs).HasDefaultValue(DateTime.MinValue);

            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => new {e.WalletId, e.Symbol, e.Date}).IsUnique();
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => new {e.WalletId, e.Symbol});
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.Symbol);
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.State);

            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.LastTs);
        }

        private void SetInterestRateCalculationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InterestRateCalculation>().ToTable(InterestRateCalculationTableName);
            
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<InterestRateCalculation>().HasKey(e => e.Id);
            
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.WalletId).HasMaxLength(64);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Symbol).HasMaxLength(64);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.NewBalance);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Apr);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Amount);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Date);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.HistoryId);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.LastTs).HasDefaultValue(DateTime.MinValue);

            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => new {e.WalletId, e.Symbol, e.Date}).IsUnique();
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => new {e.WalletId, e.Symbol});
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => e.Symbol);
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => e.IndexPrice);

            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => e.LastTs);
        }

        private void SetInterestRateSettingsEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InterestRateSettings>().ToTable(InterestRateSettingsTableName);
            
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<InterestRateSettings>().HasKey(e => e.Id);
            
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.WalletId).HasMaxLength(64);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.Asset).HasMaxLength(64);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.RangeFrom);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.RangeTo);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.Apr);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.Apy);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.DailyLimitInUsd);
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.LastTs).HasDefaultValue(DateTime.MinValue);

            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => new {e.WalletId, e.Asset, e.RangeFrom, e.RangeTo}).IsUnique();
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => new {e.WalletId, e.Asset});
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => e.Asset);

            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => e.LastTs);
        }

        private void SetInterestRateStatesEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InterestRateState>().ToTable(InterestRateStateTableName);
            
            modelBuilder.Entity<InterestRateState>().HasKey(e => new {e.AssetId, e.WalletId});
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => e.WalletId);

        }
        

        public async Task UpdateIndexPrice(IEnumerable<IndexPriceEntity> prices)
        {
            await Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {Schema}.{IndexPriceTableName};");
            
            IndexPriceCollection.AddRange(prices);
            await SaveChangesAsync();
        }
        
        public List<InterestRatePaid> GetPaidByFilter(long lastId, int batchSize, string assetFilter,
            string walletFilter, DateTime dateFilter, PaidState stateFilter, long historyFilter)
        {
            IQueryable<InterestRatePaid> paid = InterestRatePaidCollection;
            if (lastId != 0)
            {
                paid = paid.Where(e => e.Id < lastId);
            }
            if (!string.IsNullOrWhiteSpace(walletFilter))
            {
                paid = paid.Where(e => e.WalletId == walletFilter);
            }
            if (!string.IsNullOrWhiteSpace(assetFilter))
            {
                paid = paid.Where(e => e.Symbol == assetFilter);
            }
            if (dateFilter != DateTime.MinValue)
            {
                paid = paid.Where(e => e.Date.Date == dateFilter.Date);
            }
            if (stateFilter != PaidState.Undefined)
            {
                paid = paid.Where(e => e.State == stateFilter);
            }
            if (historyFilter != 0)
            {
                paid = paid.Where(e => e.HistoryId == historyFilter);
            }
            paid = paid
                .OrderByDescending(trade => trade.Id)
                .Take(batchSize);
            Console.WriteLine(paid.ToQueryString());
            return paid.ToList();
        }
        
        public List<InterestRateCalculation> GetCalculationByFilter(long lastId, int batchSize, string assetFilter,
            string walletFilter, DateTime dateFilter, long historyFilter)
        {
            IQueryable<InterestRateCalculation> calculations = InterestRateCalculationCollection;
            if (lastId != 0)
            {
                calculations = calculations.Where(e => e.Id < lastId);
            }
            if (!string.IsNullOrWhiteSpace(walletFilter))
            {
                calculations = calculations.Where(e => e.WalletId == walletFilter);
            }
            if (!string.IsNullOrWhiteSpace(assetFilter))
            {
                calculations = calculations.Where(e => e.Symbol == assetFilter);
            }
            if (dateFilter != DateTime.MinValue)
            {
                calculations = calculations.Where(e => e.Date.Date == dateFilter.Date);
            }
            if (historyFilter != 0)
            {
                calculations = calculations.Where(e => e.HistoryId == historyFilter);
            }
            calculations = calculations
                .OrderByDescending(trade => trade.Id)
                .Take(batchSize);
            
            Console.WriteLine(calculations.ToQueryString());
            
            return calculations.ToList();
        }
        public List<CalculationHistory> GetCalculationHistory()
        {
            return CalculationHistoryCollection.ToList();
        }
        public List<PaidHistory> GetPaidHistory()
        {
            return PaidHistoryCollection.ToList();
        }
        public CalculationHistory GetLastCalculationHistory()
        {
            return CalculationHistoryCollection.OrderByDescending(e => e.CompletedDate).Take(1).FirstOrDefault();
        }
        public PaidHistory GetLastPaidHistory()
        {
            return PaidHistoryCollection.OrderByDescending(e => e.CreatedDate).Take(1).FirstOrDefault();
        }
        public List<InterestRatePaid> GetTop100PaidToProcess()
        {
            return InterestRatePaidCollection
                .Where(e => e.State == PaidState.New || 
                            e.State == PaidState.Retry)
                .OrderBy(e => e.Date)
                .Take(100)
                .ToList();
        }

        public List<InterestRateSettings> GetSettings()
        {
            return InterestRateSettingsCollection.ToList();
        }
        public async Task UpsertSettings(InterestRateSettings settings)
        {
            await InterestRateSettingsCollection
                .Upsert(settings)
                .On(e => new {e.WalletId, e.Asset, e.RangeFrom, e.RangeTo})
                .RunAsync();
        }
        public async Task UpsertSettingsList(List<InterestRateSettings> settingsList)
        {
            await InterestRateSettingsCollection
                .UpsertRange(settingsList)
                .On(e => new {e.WalletId, e.Asset, e.RangeFrom, e.RangeTo})
                .RunAsync();
        }
        public async Task RemoveSettings(InterestRateSettings settings)
        {
            var entity = InterestRateSettingsCollection
                .FirstOrDefault(e => 
                    e.WalletId == settings.WalletId &&
                    e.Asset == settings.Asset &&
                    e.RangeFrom == settings.RangeFrom &&
                    e.RangeTo == settings.RangeTo);
            if (entity != null)
            {
                InterestRateSettingsCollection.Remove(entity);
                await SaveChangesAsync();
            }
        }
        public async Task ExecCalculationAsync(DateTime date, ILogger logger)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, @"Scripts/", "CalculationScript.sql");
                using var script =
                    new StreamReader(path);
                var scriptBody = await script.ReadToEndAsync();
                var dateArg = $"{date.Year}-{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}" +
                              $" {date.Hour.ToString().PadLeft(2, '0')}:{date.Minute.ToString().PadLeft(2, '0')}:{date.Second.ToString().PadLeft(2, '0')}";
                var sqlText = scriptBody.Replace("${dateArg}", dateArg);

                logger.LogInformation($"ExecCalculationAsync start with date {dateArg}");
                await Database.ExecuteSqlRawAsync(sqlText);
                logger.LogInformation($"ExecCalculationAsync finish with date {dateArg}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
        public async Task ExecPaidAsync(DateTime dateFrom, DateTime dateTo, ILogger logger)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, @"Scripts/", "PaidScript.sql");
                using var script =
                    new StreamReader(path);
                var scriptBody = await script.ReadToEndAsync();
                
                var dateFromString = $"{dateFrom.Year}-{dateFrom.Month.ToString().PadLeft(2, '0')}-{dateFrom.Day.ToString().PadLeft(2, '0')}" +
                                     $" {dateFrom.Hour.ToString().PadLeft(2, '0')}:{dateFrom.Minute.ToString().PadLeft(2, '0')}:{dateFrom.Second.ToString().PadLeft(2, '0')}";
                scriptBody = scriptBody.Replace("${dateFrom}", dateFromString);
                
                var dateToString = $"{dateTo.Year}-{dateTo.Month.ToString().PadLeft(2, '0')}-{dateTo.Day.ToString().PadLeft(2, '0')}" +
                                     $" {dateTo.Hour.ToString().PadLeft(2, '0')}:{dateTo.Minute.ToString().PadLeft(2, '0')}:{dateTo.Second.ToString().PadLeft(2, '0')}";
                scriptBody = scriptBody.Replace("${dateTo}", dateToString);
                
                logger.LogInformation($"ExecPaidAsync start with date from: {dateFromString} and date to: {dateToString}");
                await Database.ExecuteSqlRawAsync(scriptBody);
                logger.LogInformation($"ExecPaidAsync finish with date from: {dateFromString} and date to: {dateToString}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
        
        public async Task ExecCurrentCalculationAsync(DateTime date, ILogger logger)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, @"Scripts/", "CurrentCalculationScript.sql");
                using var script =
                    new StreamReader(path);
                var scriptBody = await script.ReadToEndAsync();
                var dateArg = $"{date.Year}-{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}" +
                              $" {date.Hour.ToString().PadLeft(2, '0')}:{date.Minute.ToString().PadLeft(2, '0')}:{date.Second.ToString().PadLeft(2, '0')}";
                var sqlText = scriptBody.Replace("${dateArg}", dateArg);

                logger.LogInformation($"ExecStateCalculationAsync start with date {dateArg}");
                await Database.ExecuteSqlRawAsync(sqlText);
                logger.LogInformation($"ExecStateCalculationAsync finish with date {dateArg}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
        
        public async Task ExecTotalCalculationAsync(ILogger logger)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, @"Scripts/", "TotalCalculationScript.sql");
                using var script =
                    new StreamReader(path);
                var scriptBody = await script.ReadToEndAsync();

                logger.LogInformation($"ExecTotalCalculationAsync start");
                await Database.ExecuteSqlRawAsync(scriptBody);
                logger.LogInformation($"ExecTotalCalculationAsync finish");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public async Task RetryPaidPeriod(DateTime createdDate)
        {
            await InterestRatePaidCollection
                .Where(e => e.Date == createdDate && e.State == PaidState.Failed)
                .ForEachAsync(e =>
                {
                    e.State = PaidState.Retry;
                    e.TransactionId = Guid.NewGuid().ToString();
                });
            await SaveChangesAsync();
        }

        public Dictionary<string, decimal> GetAccumulatedRates(string walletId)
        {
            var lastPaid = GetLastPaidHistory();

            var rates = InterestRateCalculationCollection
                .Where(e => e.WalletId == walletId && e.Date > lastPaid.RangeTo)
                .ToList();

            var ratesDictionary = new Dictionary<string, decimal>();
            
            rates.ForEach(e =>
            {
                if (ratesDictionary.TryGetValue(e.Symbol, out _))
                {
                    ratesDictionary[e.Symbol] += e.Amount;
                }
                else
                {
                    ratesDictionary.Add(e.Symbol, e.Amount);
                }
            });

            return ratesDictionary;
        }

        public async Task<Dictionary<string, (decimal current, decimal total)>> GetEarnStates(string walletId)
        {
           return await InterestRateStates.Where(t => t.WalletId == walletId)
                .ToDictionaryAsync(t => t.AssetId, t => (t.CurrentEarnAmount, t.TotalEarnAmount));
        }
        
        public async Task<Dictionary<string, decimal>> GetCalculatedAmountAsync(DateTime start, DateTime end)
        {
            var list  = await InterestRateCalculationCollection
                .Where(a => a.Date >= start && a.Date <= end)
                .GroupBy(a => a.Symbol)
                .Select(a => new {Symbol = a.Key, Amount = a.Sum(x => x.Amount)})
                .ToListAsync();

            return list.ToDictionary(k => k.Symbol, v => v.Amount);
        }
        
        public async Task<Dictionary<string, decimal>> GetPaidAmountAsync(DateTime start, DateTime end)
        {
            var list  = await InterestRatePaidCollection
                .Where(a => a.Date >= start && a.Date <= end && a.State == PaidState.Completed)
                .GroupBy(a => a.Symbol)
                .Select(a => new {Symbol = a.Key, Amount = a.Sum(x => x.Amount)})
                .ToListAsync();

            return list.ToDictionary(k => k.Symbol, v => v.Amount);
        }
    }
}
