using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        private DbSet<InterestRateSettings> InterestRateSettingsCollection { get; set; }
        private DbSet<InterestRateCalculation> InterestRateCalculationCollection { get; set; }
        
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
            
            base.OnModelCreating(modelBuilder);
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
                using var script =
                    new StreamReader("..\\Service.InterestManager.Postgres\\Scripts\\CalculationScript.sql");
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
