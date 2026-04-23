using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{

        public class UserDto
        {
            public long? Id { get; set; }
            public string FullName { get; set; }
            public string Username { get; set; }
            public string PasswordHash { get; set; }
            public string Role { get; set; }
            public string TenantId { get; set; }

            // Fields for the List view
            public string? BusinessName { get; set; }
            public DateTime? CreatedAt { get; set; }

            // Extra field for the Owner's workflow (Subscription)
            public int DurationMonths { get; set; }
        }

    
}
