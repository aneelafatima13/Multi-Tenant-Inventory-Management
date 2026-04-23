using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    // 1. Tenants Entity
    public class Tenant
    {
        [Key] // Marks this as the Primary Key
        [StringLength(6)]
        public string Id { get; set; }
        public string BusinessName { get; set; }
        public string SubscriptionPlan { get; set; } = "Basic";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public int Status { get; set; }

        [NotMapped] // Tells EF Core NOT to look for this in the SQL table
        public string StatusDisplayText => Status == 1 ? "Active" : "Disabled";

        // A Tenant has many products, users, and subscriptions
        public ICollection<User> Users { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }

    // 2. User Entity
    public class User : ITenantEntity
    {
        public long? Id { get; set; }
        public string TenantId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }

        public Tenant Tenant { get; set; }
    }

    // 3. Product Entity
    public class Product : ITenantEntity
    {
        public long? Id { get; set; }
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public int StockLevel { get; set; }
        public int LowStockThreshold { get; set; }

        public Tenant Tenant { get; set; }
    }

    // 4. Subscription Entity
    public class Subscription : ITenantEntity
    {
        public long? Id { get; set; }
        public string TenantId { get; set; }
        public string CustomerName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = "Active";

        public Tenant Tenant { get; set; }
    }
}
