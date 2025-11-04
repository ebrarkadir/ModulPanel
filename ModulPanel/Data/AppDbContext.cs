using Microsoft.EntityFrameworkCore;
using ModulPanel.Entities;
using ModulPanel.Enums;

namespace ModulPanel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 🔹 Tablolar
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Module> Modules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 UserRole enum'unu string olarak sakla
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(
                    v => v.ToString(),
                    v => (UserRole)Enum.Parse(typeof(UserRole), v)
                );

            // 🔹 Username benzersiz olmalı
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 🔹 UserPermission -> User ilişkisi (1:N) ve cascade delete aktif
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Module tablosu (opsiyonel, isActive kontrolü varsa ekle)
            modelBuilder.Entity<Module>()
                .Property(m => m.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Log>()
                .Property(l => l.CreatedAt)
                .ValueGeneratedOnAdd(); // 🔹 EF otomatik tarih basacak, DB default yok


        }
    }
}
