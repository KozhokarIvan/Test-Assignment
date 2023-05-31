using Microsoft.EntityFrameworkCore;
using WebAPI.Database.Models;

namespace WebAPI.Database
{
    public class UsersDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.Guid);
            base.OnModelCreating(modelBuilder);
        }
    }
}
