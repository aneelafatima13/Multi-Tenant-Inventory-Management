using Common.Interfaces;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Common;

namespace DAL
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _currentTenantId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _currentTenantId = tenantService.GetTenantId();
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

            // --- User Configuration (BigInt ID) ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Identity(1,1)
                entity.Property(u => u.TenantId).HasMaxLength(6).IsRequired();

                // Relationship: One Tenant -> Many Users
                entity.HasOne(u => u.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Subscription Configuration (BigInt ID) ---
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(s => s.TenantId).HasMaxLength(6).IsRequired();

                // Fix for the SQL Default value conflict (Mapping Int to SQL Default)
                entity.Property(s => s.Status).HasDefaultValue(1);

                // Relationships
                entity.HasOne(s => s.Tenant)
                      .WithMany(t => t.Subscriptions)
                      .HasForeignKey(s => s.TenantId);

                entity.HasOne(s => s.User)
                      .WithMany() // Or u.Subscriptions if added to User entity
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // --- Global Multi-Tenant Query Filter ---
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Ensure the entity has a TenantId property before applying filter
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
                }
            }
        }

        private LambdaExpression ConvertFilterExpression(Type type)
        {
            var parameter = Expression.Parameter(type, "e");
            var property = Expression.Property(parameter, "TenantId");
            var tenantIdConstant = Expression.Constant(_currentTenantId, typeof(string));
            var body = Expression.Equal(property, tenantIdConstant);

            return Expression.Lambda(body, parameter);
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