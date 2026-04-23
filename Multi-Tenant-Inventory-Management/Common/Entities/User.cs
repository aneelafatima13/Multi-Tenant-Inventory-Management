using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;

namespace Common.Entities
{
    public class User : ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } // bigint NOT NULL IDENTITY(1,1)

        [Required]
        [StringLength(6)]
        public string TenantId { get; set; } // nvarchar(6) NOT NULL

        [StringLength(50)]
        public string? FullName { get; set; } // nvarchar(50) NULL

        [Required]
        [StringLength(50)]
        public string Username { get; set; } // nvarchar(50) NOT NULL

        [Required]
        public string PasswordHash { get; set; } // nvarchar(max) NOT NULL

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // nvarchar(20) NOT NULL (Owner, Admin, Staff)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public long CreatedBy { get; set; } // bigint NOT NULL

        public DateTime? ModifiedAt { get; set; }

        public long? ModifiedBy { get; set; }

        // Navigation Properties
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        [NotMapped]
        public int DurationMonths { get; set; }

        [NotMapped]
        public string BusinessName { get; set; }
    }


    public class UserListView
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BusinessName { get; set; }
        public string TenantId { get; set; }
    }
}