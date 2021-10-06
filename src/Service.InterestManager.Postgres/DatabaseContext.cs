using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.IntrestManager.Domain.Models;

namespace Service.InterestManager.Postrges
{
    public class DatabaseContext : DbContext
    {
        public const string Schema = "interest_manager";
        
        private const string InterestRateSettingsTableName = "interestratesettings";
        private const string InterestRateCalculationTableName = "interestratecalculation";
        private const string InterestRatePaidTableName = "interestratepaid";
        private const string CalculationHistoryTableName = "calculationhistory";
        private const string PaidHistoryTableName = "paidhistory";
        
        private DbSet<InterestRateSettings> InterestRateSettingsCollection { get; set; }
        private DbSet<InterestRateCalculation> InterestRateCalculationCollection { get; set; }
        private DbSet<InterestRatePaid> InterestRatePaidCollection { get; set; }
        private DbSet<CalculationHistory> CalculationHistoryCollection { get; set; }
        private DbSet<PaidHistory> PaidHistoryCollection { get; set; }
        
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        public static ILoggerFactory LoggerFactory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (LoggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(LoggerFactory).EnableSensitiveDataLogging();
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetInterestRateSettingsEntity(modelBuilder);
            SetInterestRateCalculationEntity(modelBuilder);
            SetInterestRatePaidEntity(modelBuilder);
            SetCalculationHistoryEntity(modelBuilder);
            SetPaidHistoryEntity(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }
        
        private void SetCalculationHistoryEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculationHistory>().ToTable(CalculationHistoryTableName);
            
            modelBuilder.Entity<CalculationHistory>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<CalculationHistory>().HasKey(e => e.Id);
            
            modelBuilder.Entity<CalculationHistory>().Property(e => e.CompletedDate);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.WalletCount);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.AmountInWalletsInUsd);
            modelBuilder.Entity<CalculationHistory>().Property(e => e.CalculatedAmountInUsd);
            
            modelBuilder.Entity<CalculationHistory>().HasIndex(e => e.CompletedDate);
        }

        private void SetPaidHistoryEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaidHistory>().ToTable(PaidHistoryTableName);
            
            modelBuilder.Entity<PaidHistory>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<PaidHistory>().HasKey(e => e.Id);
            
            modelBuilder.Entity<PaidHistory>().Property(e => e.CompletedDate);
            modelBuilder.Entity<PaidHistory>().Property(e => e.RangeFrom);
            modelBuilder.Entity<PaidHistory>().Property(e => e.RangeTo);
            modelBuilder.Entity<PaidHistory>().Property(e => e.WalletCount);
            modelBuilder.Entity<PaidHistory>().Property(e => e.TotalPaidInUsd);
            
            modelBuilder.Entity<PaidHistory>().HasIndex(e => e.CompletedDate);
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
            
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => new {e.WalletId, e.Symbol, e.Date}).IsUnique();
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => new {e.WalletId, e.Symbol});
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.Symbol);
            modelBuilder.Entity<InterestRatePaid>().HasIndex(e => e.State);
        }

        private void SetInterestRateCalculationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InterestRateCalculation>().ToTable(InterestRateCalculationTableName);
            
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<InterestRateCalculation>().HasKey(e => e.Id);
            
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.WalletId).HasMaxLength(64);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Symbol).HasMaxLength(64);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.NewBalance);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Apy);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Amount);
            modelBuilder.Entity<InterestRateCalculation>().Property(e => e.Date);
            
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => new {e.WalletId, e.Symbol, e.Date}).IsUnique();
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => new {e.WalletId, e.Symbol});
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<InterestRateCalculation>().HasIndex(e => e.Symbol);
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
            modelBuilder.Entity<InterestRateSettings>().Property(e => e.Apy);
            
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => new {e.WalletId, e.Asset, e.RangeFrom, e.RangeTo}).IsUnique();
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => new {e.WalletId, e.Asset});
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => e.WalletId);
            modelBuilder.Entity<InterestRateSettings>().HasIndex(e => e.Asset);
        }
        
        public async Task SavePaidHistory(PaidHistory entity)
        {
            PaidHistoryCollection.Add(entity);
            await SaveChangesAsync();
        }

        public async Task SaveCalculationHistory(CalculationHistory entity)
        {
            CalculationHistoryCollection.Add(entity);
            await SaveChangesAsync();
        }
        
        public InterestRateCalculation GetLastCalculation()
        {
            return InterestRateCalculationCollection.OrderByDescending(e => e.Date).Take(1).FirstOrDefault();
        }
        public InterestRatePaid GetLastPaid()
        {
            return InterestRatePaidCollection.OrderByDescending(e => e.Date).Take(1).FirstOrDefault();
        }

        public List<InterestRatePaid> GetNewPaidCollection()
        {
            return InterestRatePaidCollection
                .Where(e => e.State == PaidState.New)
                .OrderBy(e => e.Date)
                .Take(100)
                .ToList();
        }

        public async Task SavePaidCollection(IEnumerable<InterestRatePaid> collection)
        {
            await InterestRatePaidCollection
                .UpsertRange(collection)
                .On(e => new {e.WalletId, e.Symbol, e.Date})
                .NoUpdate()
                .RunAsync();
        }

        public List<InterestRateCalculation> GetInterestRateCalculationByDate(DateTime dateFrom, DateTime dateTo)
        {
            return InterestRateCalculationCollection
                .Where(e => e.Date >= dateFrom && e.Date < dateTo)
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
                              $" {date.Hour.ToString().PadLeft(2, '0')}:{date.Minute.ToString().PadLeft(2, '0')}";
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
    }
}
