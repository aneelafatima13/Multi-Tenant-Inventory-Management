
using Common.Interfaces;


namespace Multi_Tenant_Inventory_Management_Web_API
{
    // Inside your API Project
    
    public class ApiTenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiTenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenantId() // Updated to return string
        {
            // The Security Guard catches the "tenant-id" header sent by the Messenger (ApiService)
            return _httpContextAccessor.HttpContext?.Request.Headers["tenant-id"].ToString() ?? "";
        }
    }
}
