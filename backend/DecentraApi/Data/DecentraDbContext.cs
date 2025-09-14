using Microsoft.EntityFrameworkCore;
using DecentraApi.Models;

namespace DecentraApi.Data
{
    public class DecentraDbContext : DbContext
    {
        public DecentraDbContext(DbContextOptions<DecentraDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Appeal> Appeals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Surname).HasColumnName("surname");
                entity.Property(e => e.Role).HasColumnName("Role");
                entity.Property(e => e.PhotoIds).HasColumnName("PhotoIds");
                entity.Property(e => e.AppealId).HasColumnName("AppealId");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                
                
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.PhotoIds).IsUnique();
                entity.HasIndex(e => e.AppealId).IsUnique();
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.ToTable("photos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.LastUpdated)
                    .HasColumnName("LastUpdated")
                    .HasDefaultValueSql("NOW()");
                entity.Property(e => e.Rust).HasColumnName("Rust");
                entity.Property(e => e.Dent).HasColumnName("Dent");
                entity.Property(e => e.Scratch).HasColumnName("Scratch");
                entity.Property(e => e.Dust).HasColumnName("Dust");
                entity.Property(e => e.Image).HasColumnName("Image");
                
            });

            modelBuilder.Entity<Appeal>(entity =>
            {
                entity.ToTable("appeals");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PhotoIds).HasColumnName("PhotoIds");
                entity.Property(e => e.Description).HasColumnName("Description");
                entity.Property(e => e.Appealed).HasColumnName("Appealed");
            });
        }
    }
}