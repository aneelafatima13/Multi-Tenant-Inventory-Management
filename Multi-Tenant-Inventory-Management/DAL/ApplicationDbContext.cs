using Common.Interfaces;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Common;

namespace DAL
{
    public class ApplicationDbContext : DbContext
    {
        // Change: Store the service to access TenantId dynamically
        private readonly ITenantService _tenantService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserListView> UserListView { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Tenant Configuration ---
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(6).IsUnicode(true);
                entity.HasIndex(e => e.BusinessName).IsUnique();
            });

            modelBuilder.Entity<UserListView>()
                .ToView("vw_UserList")
                .HasNoKey();

            // --- User Configuration ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.TenantId).HasMaxLength(6).IsRequired();

                // Explicitly map the relationship to prevent "TenantId1"
                entity.HasOne(u => u.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Product Configuration ---
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.TenantId).HasMaxLength(6).IsRequired();

            });

            // --- Subscription Configuration ---
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(s => s.TenantId).HasMaxLength(6).IsRequired();
                entity.Property(s => s.Status).HasDefaultValue(1);

                entity.HasOne(s => s.Tenant)
                      .WithMany(t => t.Subscriptions)
                      .HasForeignKey(s => s.TenantId);

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // --- Global Multi-Tenant Query Filter (Dynamic Fix) ---
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Use the dynamic filter helper
                    var method = typeof(ApplicationDbContext)
                        .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.MakeGenericMethod(entityType.ClrType);

                    method?.Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        // This ensures the filter is evaluated every request, not just once at startup
        private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _tenantService.GetTenantId());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:Multi-Tenant-Inventory-Management-Db");
            }
        }
    }
}