using Common.Interfaces;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace DAL
{
    public class ApplicationDbContext : DbContext
    {
        // 1. Update the private field
        private readonly string _currentTenantId; // Changed from Guid

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            // 2. Get the string ID
            _currentTenantId = tenantService.GetTenantId();
        }

        // 3. Update the filter helper
        private LambdaExpression ConvertFilterExpression(Type type)
        {
            var parameter = Expression.Parameter(type, "e");
            var property = Expression.Property(parameter, "TenantId");

            // Ensure the constant is a string
            var tenantIdConstant = Expression.Constant(_currentTenantId, typeof(string));
            var body = Expression.Equal(property, tenantIdConstant);

            return Expression.Lambda(body, parameter);
        }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Tenant>()
   .Property(t => t.Id)
   .HasMaxLength(6)
   .IsUnicode(true);

            // Define the relationship for Tenant -> Many Entities
            modelBuilder.Entity<Tenant>().HasMany(t => t.Products).WithOne(p => p.Tenant).HasForeignKey(p => p.TenantId);
            modelBuilder.Entity<Tenant>().HasMany(t => t.Users).WithOne(u => u.Tenant).HasForeignKey(u => u.TenantId);
            modelBuilder.Entity<Tenant>().HasMany(t => t.Subscriptions).WithOne(s => s.Tenant).HasForeignKey(s => s.TenantId);
           
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
                }
            }
        }
       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This is a safety fallback. 
                // In a real N-Tier app, Program.cs should handle this, 
                // but this stops the "Not Initialized" error immediately.
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:Multi-Tenant-Inventory-Management-Db");
            }
        }
    }
}
