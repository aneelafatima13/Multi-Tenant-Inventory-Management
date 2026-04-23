using Common;
using Microsoft.AspNetCore.Mvc;

namespace Multi_TenantInventory_SubscriptionManager.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApiService _apiService;

        public LoginController(ApiService apiService)
        {
            _apiService = apiService;
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return Json(new { success = false, msg = "Username and Password are required." });

            // 1. Hardcoded Owner Bypass (Keep as is for system admin)
            if (model.Email == "owner" && model.Password == "admin123")
            {
                HttpContext.Session.SetString("UserRole", "Owner");
                HttpContext.Session.SetString("TenantId", "0"); // System Tenant
                HttpContext.Session.SetString("Username", "SystemAdmin");
                return Json(new { success = true, msg = "Welcome, Owner!", redirect = "/Dashboard/Dashboard" });
            }

            try
            {
                var response = await _apiService.PostAsync<ApiResponse>("api/Auth/login", model);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // 2. Store Token in Session (or Cookie)
                    HttpContext.Session.SetString("JWToken", response.Token);
                    HttpContext.Session.SetString("UserRole", response.Role);
                    HttpContext.Session.SetString("TenantId", response.TenantId);

                    return Json(new { success = true, msg = "Login Successful", redirect = "/Dashboard/Dashboard" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Login failed: " + ex.Message });
            }

            return Json(new { success = false, msg = "Invalid Username or Password." });
        }
    }
}
