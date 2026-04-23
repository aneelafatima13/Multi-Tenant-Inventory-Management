using BAL;
using DAL;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace Multi_Tenant_Inventory_Management_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantApiController : ControllerBase
    {
        private readonly TenantBAL _tenantBal;

        public TenantApiController(TenantBAL tenantBal)
        {
            _tenantBal = tenantBal;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _tenantBal.GetAllTenantsAsync());

        [HttpPost("Upsert")]
        public async Task<IActionResult> Post([FromBody] TenantDto model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.BusinessName))
                    return BadRequest("Invalid tenant data.");

                var success = await _tenantBal.UpsertTenantAsync(model);
                return success ? Ok(new { message = "Saved successfully" })
                               : StatusCode(500, "A database error occurred while saving.");
            }
            catch (Exception ex)
            {
                // In production, use a logger here: _logger.LogError(ex, "Error in Upsert");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int skip, int take, string? search = "")
        {
            var result = await _tenantBal.GetPagedTenantsAsync(skip, take, search ?? "");
            return Ok(result);
        }

        [HttpPost("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var success = await _tenantBal.ToggleTenantStatusAsync(id);
            return success ? Ok() : BadRequest();
        }
    }
}
