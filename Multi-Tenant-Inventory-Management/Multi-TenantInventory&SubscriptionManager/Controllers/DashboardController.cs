using Microsoft.AspNetCore.Mvc;

namespace Multi_TenantInventory_SubscriptionManager.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var tenantName = HttpContext.Session.GetString("TenantName");

            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login");

            ViewBag.Message = $"Welcome, {role} of {tenantName}";
            return View();
        }
    }


}
