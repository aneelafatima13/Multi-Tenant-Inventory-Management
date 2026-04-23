using Common;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Multi_TenantInventory_SubscriptionManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApiService _apiService;

        public UsersController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult ManageUsers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] UserDto model)
        {
            try
            {
                var response = await _apiService.PostAsync("api/UserAPI/Register", model);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "User registered successfully!" });
                }

                // Read the error text sent by the API (e.g., "Email already exists" or the Exception message)
                var errorDetail = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Connection to API failed: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetUsersPaged()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            // Call API for paged users
            var response = await _apiService.GetPagedAsync<UserListView>("api/UserAPI/paged", start, length, searchValue);

            return Json(new
            {
                draw = draw,
                recordsFiltered = response.TotalCount,
                recordsTotal = response.TotalCount,
                data = response.Items
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetBusinessList()
        {
            // Call the Web API endpoint
            var tenants = await _apiService.GetAsync<List<TenantDto>>("api/TenantAPI/list");
            return Json(tenants ?? new List<TenantDto>());
        }


    }
}
