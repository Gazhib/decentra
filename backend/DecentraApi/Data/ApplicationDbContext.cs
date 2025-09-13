using Microsoft.EntityFrameworkCore;
using TaxiCarAPI.Models;

namespace TaxiCarAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Appeal> Appeals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.Property(e => e.PhotoIds)
                    .HasColumnType("integer[]");
            });

            // Photo entity configuration
            modelBuilder.Entity<Photo>(entity =>
            {
                entity.Property(e => e.LastUpdated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Appeal entity configuration
            modelBuilder.Entity<Appeal>(entity =>
            {
                entity.Property(e => e.PhotoIds)
                    .HasColumnType("integer[]");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}