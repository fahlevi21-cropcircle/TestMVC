using Microsoft.EntityFrameworkCore;
using TestMVC.Models;

namespace TestMVC.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> opt) : base(opt) { }
        public DbSet<Product> Product { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<UserToken> UserToken { get; set; }
        public DbSet<Reminder> Reminder { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserToken>().HasKey(x => x.UserId);
        }
    }
}
