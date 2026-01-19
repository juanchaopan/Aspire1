using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
namespace WebApplication1
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.Name).IsRequired().HasMaxLength(128);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(64);
                entity.Property(u => u.DateOfBirth).IsRequired();
            });
        }
    }
}