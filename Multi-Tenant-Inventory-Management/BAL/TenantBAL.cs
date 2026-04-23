using Common;
using Common.Entities;
using DAL;
namespace BAL
{
    public class TenantBAL
    {
        private readonly TenantDAL _tenantDal;
        
        public TenantBAL(TenantDAL tenantDal) => _tenantDal = tenantDal;

        public async Task<PagedResponse<Tenant>> GetPagedTenantsAsync(int skip, int take, string search)
        {
            var result = await _tenantDal.GetPagedTenantsAsync(skip, take, search);
            return new PagedResponse<Tenant>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };
        }

        public async Task<List<Tenant>> GetAllTenantsAsync() => await _tenantDal.GetAllTenantsAsync();

        public async Task<bool> UpsertTenantAsync(TenantDto dto)
        {
            var tenant = new Tenant
            {
                Id = dto.Id ?? string.Empty,
                BusinessName = dto.BusinessName,
                SubscriptionPlan = dto.SubscriptionPlan, // Handled in DAL update usually
            };
            return await _tenantDal.SaveTenantAsync(tenant);
        }

        public async Task<bool> ToggleTenantStatusAsync(string id)
        {
            var tenant = await _tenantDal.GetByIdAsync(id);
            if (tenant == null) return false;

            int newStatus = (tenant.Status == 1) ? 0 : 1;

            // Call a specific partial update method in your DAL
            return await _tenantDal.UpdateStatusOnlyAsync(id, newStatus);
        }

        public async Task<IEnumerable<object>> GetTenantsForDropdownAsync()
        {
            var tenants = await _tenantDal.GetAllTenantsAsync();

            // Perform the projection/mapping here
            return tenants.Select(t => new
            {
                t.Id,
                t.BusinessName
            });
        }
    }
}