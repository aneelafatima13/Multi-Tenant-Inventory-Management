using Common;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Multi_TenantInventory_SubscriptionManager.Controllers
{
    public class TenantsController : Controller
    {
        private readonly ApiService _apiService;

        public TenantsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult ManageTenants() => View();

        // This endpoint is called by your jQuery AJAX
        [HttpPost] // Changed to POST to match JS
        public async Task<IActionResult> GetTenants()
        {
            // Extract parameters from the request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // Call API (passing pagination parameters)
            var response = await _apiService.GetPagedAsync<Tenant>("api/TenantApi/paged", skip, pageSize, searchValue);

            return Json(new
            {
                draw = draw,
                recordsFiltered = response.TotalCount,
                recordsTotal = response.TotalCount,
                data = response.Items
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveTenant([FromBody] TenantDto model)
        {
            try
            {
                var response = await _apiService.PostAsync("api/TenantApi/Upsert", model);

                if (response.IsSuccessStatusCode)
                    return Ok();

                // Read the error message from the API if available
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "API Service is unavailable.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            // Calling your API to flip the bit
            var response = await _apiService.PostAsync<object>($"api/TenantApi/ToggleStatus/{id}", null);
            return response.IsSuccessStatusCode ? Ok() : StatusCode(500);
        }
    }
}