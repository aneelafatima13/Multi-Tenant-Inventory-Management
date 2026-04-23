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
            if (model == null) return Json(new { success = false, msg = "Invalid request." });

            
            if (model.Email == "owner" && model.Password == "admin123")
            {
                
                HttpContext.Session.SetString("UserRole", "Owner");
                HttpContext.Session.SetString("TenantName", "System Global");

                return Json(new { success = true, msg = "Welcome, Owner!", redirect = "/Dashboard/Dashboard" });
            }

            
            try
            {
                //var result = await _apiService.PostAsync<LoginViewModel, ApiResponse>("account/login", model);

                //if (result != null && result.Success)
                //{
                //    HttpContext.Session.SetString("UserRole", "TenantAdmin");
                //    HttpContext.Session.SetString("JWToken", result.Token); // Contains the TenantId

                //    return Json(new { success = true, msg = "Login Successful!", redirect = "/Dashboard/Dashboard" });
                //}
            }
            catch (Exception)
            {
                return Json(new { success = false, msg = "API Connection Error." });
            }

            return Json(new { success = false, msg = "Invalid Email or Password." });
        }

    }
}
