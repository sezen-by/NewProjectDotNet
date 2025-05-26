using Microsoft.EntityFrameworkCore;
using RateLimiter.Models;

namespace RateLimiter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<WhitelistedUser> WhitelistedUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum'ı string olarak sakla
            modelBuilder.Entity<User>()
                .Property(e => e.Role)
                .HasConversion<string>();

            // User tablosu konfigürasyonu
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasDefaultValue(UserRole.user);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                
                // Username unique olsun
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // WhitelistedUser tablosu konfigürasyonu
            modelBuilder.Entity<WhitelistedUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Foreign key relationship
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Bir kullanıcı sadece bir kez whitelist'e alınabilir
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Seed data - İlk admin kullanıcısı ve whitelist
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    // Sabit hash değeri kullandık - "admin123" şifresinin hash'i
                    Password = "$2a$11$M5Z/wPetuMFvzmorEESQTOzxB7IPn8C1R78PgLGGrEJtSPd.Zwuby",
                    Role = UserRole.admin,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) // Sabit tarih
                }
            );

            modelBuilder.Entity<WhitelistedUser>().HasData(
                new WhitelistedUser
                {
                    Id = 1,
                    UserId = 1,
                    Username = "admin",
                    Description = "System Administrator",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Sabit tarih
                    IsActive = true
                }
            );
        }
    }
}