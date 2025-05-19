using Microsoft.EntityFrameworkCore;
using Entity;
using Entity.DbSet;
using Entity.Repository;
using Managers;

namespace DataContext
{
    public class AppDbContext : DbContext
    {
        public AccountRepository AccountRepo { get; set; }
        public ItemRepository ItemRepo { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options, GameDataManager gameDataManager) : base(options)
        {
            AccountRepo = new AccountRepository(this, gameDataManager);
            ItemRepo = new ItemRepository(this, gameDataManager);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountDb>().ToTable("Account");
            modelBuilder.Entity<ItemDb>().ToTable("Item")
                .HasOne<AccountDb>()
                .WithMany(a => a.items)
                .HasForeignKey(i => i.account_id);
        }
    }
}
