using Microsoft.EntityFrameworkCore;
using ModulPanel.Entities;

namespace ModulPanel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 🔹 Veritabanı tabloları
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 User <-> UserPermission (1 - N ilişkisi)
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Unique Username (her kullanıcı adı tekil olmalı)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();


        }
    }
}
