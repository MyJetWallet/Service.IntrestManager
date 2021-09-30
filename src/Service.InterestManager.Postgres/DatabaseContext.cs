using System.Collections.Generic;
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
        
        private DbSet<InterestRateSettings> InterestRateSettingsCollection { get; set; }
        
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
            
            base.OnModelCreating(modelBuilder);
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
    }
}