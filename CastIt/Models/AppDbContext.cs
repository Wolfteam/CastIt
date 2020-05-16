using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models.Entities;
using CastIt.Models.EntitiesConfiguration;
using Microsoft.EntityFrameworkCore;
using MvvmCross.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Models
{
    public class AppDbContext : DbContext
    {
        public const string CurrentAppMigration = "v1";
        public const string DatabaseName = "CastIt.db";

        public DbSet<FileItem> Files { get; set; }
        public DbSet<PlayList> PlayLists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databasePath = FileUtils.GetDbConnectionString();
            optionsBuilder.UseSqlite(databasePath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileItemConfigurationType).Assembly);
            var playList = new PlayList
            {
                Id = 1,
                Name = "Default",
            };
            modelBuilder.Entity<PlayList>().HasData(playList);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

            var now = DateTime.Now;

            foreach (var entry in modifiedEntries)
            {
                if (!(entry.Entity is IBaseEntity))
                    continue;

                var entity = entry.Entity as IBaseEntity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }
                else
                {
                    entity.UpdatedAt = now;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        public static void Init(IAppSettingsService appSettings, IMvxLog logger)
        {
            logger.Info($"Checking if the lastest migration = {CurrentAppMigration} is applied");
            try
            {
                if (appSettings.CurrentAppMigration == CurrentAppMigration)
                {
                    logger.Info("Lastest migration is applied...");
                    return;
                }
                logger.Info("Migration is not applied... Aplying it...");
                using (var context = new AppDbContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.Migrate();

                    appSettings.CurrentAppMigration = CurrentAppMigration;
                }
                logger.Info("Migration was successfully applied");
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
            }
        }
    }
}
