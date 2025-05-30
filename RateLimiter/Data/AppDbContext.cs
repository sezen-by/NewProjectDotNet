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

            modelBuilder.Entity<User>()
                .Property(e => e.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasDefaultValue(UserRole.user);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<WhitelistedUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "$2a$11$M5Z/wPetuMFvzmorEESQTOzxB7IPn8C1R78PgLGGrEJtSPd.Zwuby",
                    Role = UserRole.admin,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
                }
            );

            modelBuilder.Entity<WhitelistedUser>().HasData(
                new WhitelistedUser
                {
                    Id = 1,
                    UserId = 1,
                    Username = "admin",
                    Description = "System Administrator",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                    IsActive = true
                }
            );
        }
    }
}