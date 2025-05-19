using DataContext;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Services.Redis;

namespace Extensions
{
    public static class DbContextExtension
    {
        public static async Task<List<EntityEntry>> SaveChangesWithTrackingAsync(this DbContext dbContext, List<EntityEntry>? existingEntries = null)
        {
            List<EntityEntry>? trackedEntries = existingEntries ?? new List<EntityEntry>();
            
            int result = 0;
            
            var relevantEntries = dbContext.ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Added ||
                                entry.State == EntityState.Modified ||
                                entry.State == EntityState.Deleted);
            
            foreach (var entry in relevantEntries)
            {
                if (!trackedEntries.Contains(entry))
                {
                    trackedEntries.Add(entry);
                }
            }
            
            await dbContext.SaveChangesAsync();
            
            return trackedEntries;
        }
        
        public static List<EntityEntry> SaveChangesWithTracking(this DbContext dbContext, List<EntityEntry>? existingEntries = null)
        {
            List<EntityEntry>? trackedEntries = existingEntries ?? new List<EntityEntry>();
            int result = 0;
            
            var relevantEntries = dbContext.ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Added ||
                                entry.State == EntityState.Modified ||
                                entry.State == EntityState.Deleted);
            
            foreach (var entry in relevantEntries)
            {
                if (!trackedEntries.Contains(entry))
                {
                    trackedEntries.Add(entry);
                }
            }

            dbContext.SaveChanges();
            
            return trackedEntries;
        }
    }
}