using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;

namespace Common.Entities
{
    public class Subscription : ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } // bigint NOT NULL IDENTITY(1,1)

        [Required]
        [StringLength(6)]
        public string TenantId { get; set; } // nvarchar(6) NOT NULL

        [Required]
        public long UserId { get; set; } // bigint NOT NULL (The Admin/User assigned to sub)

        [Required]
        public DateTime ExpiryDate { get; set; } // datetime2(7) NOT NULL

        public int Status { get; set; } = 1; // int (1 for Active, 0 for Expired/Disabled)

        // Navigation Properties
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}