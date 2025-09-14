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
                // keep column names lowercase to match typical Postgres conventions
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.PhotoIds).HasColumnName("photo_ids");
                entity.Property(e => e.AppealId).HasColumnName("appeal_id");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                
                
                // Only index the phone (unique). Do not create unique indexes on array or foreign key columns.
                entity.HasIndex(e => e.Phone).IsUnique();
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
                // New JSON/text columns for damage classes and masks
                entity.Property(e => e.DamageClasses).HasColumnName("DamageClasses");
                entity.Property(e => e.Masks).HasColumnName("Masks");
                
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