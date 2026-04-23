using Microsoft.AspNetCore.Mvc;

namespace Multi_TenantInventory_SubscriptionManager.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult ManageUsers()
        {
            return View();
        }
    }
}
