using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ApiResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string TenantId { get; set; }
        public string Message { get; set; } // Optional, if you send it from API
    }
}
