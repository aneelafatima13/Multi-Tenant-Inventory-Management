using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Required for Session extensions

namespace Multi_TenantInventory_SubscriptionManager.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApiService _apiService;

        public ProductsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Helper property to get TenantId from Session
        private string CurrentTenantId => HttpContext.Session.GetString("TenantId") ?? "";
        private string CurrentUserId => HttpContext.Session.GetString("UserId") ?? "";

        public IActionResult ManageProducts()
        {
            // Optional: Redirect to login if session is lost
            if (string.IsNullOrEmpty(CurrentTenantId))
                return RedirectToAction("Login", "Login");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetProducts()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Use the session-based TenantId
            string tenantId = CurrentTenantId;

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int page = (skip / pageSize) + 1;

            var endpoint = $"api/ProductsApi/paged?page={page}&pageSize={pageSize}&tenantId={tenantId}";
            var response = await _apiService.GetPagedAsync<Product>(endpoint, skip, pageSize, searchValue);

            return Json(new
            {
                draw = draw,
                recordsFiltered = response?.TotalCount ?? 0,
                recordsTotal = response?.TotalCount ?? 0,
                data = response?.Items ?? new List<Product>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct([FromBody] Product model)
        {
            try
            {
                
                if (string.IsNullOrEmpty(CurrentTenantId))
                    return Unauthorized("Session expired. Please log in again.");

                model.TenantId = CurrentTenantId;
                if (model.Id == 0)
                {
                    model.CreatedbyId = Convert.ToInt64(CurrentUserId);
                }
                else
                {
                    model.ModifiedbyId = Convert.ToInt64(CurrentUserId);
                }
                var response = await _apiService.PostAsync("api/ProductsApi/Save", model);

                if (response.IsSuccessStatusCode)
                    return Ok();

                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error connecting to the product service.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(long id, int newStock)
        {
            // URL: api/ProductsApi/UpdateStock/5?newStock=50&tenantId=XYZ
            var url = $"api/ProductsApi/UpdateStock/{id}?newStock={newStock}&tenantId={CurrentTenantId}";
            var response = await _apiService.PostAsync<object>(url, null);

            return response.IsSuccessStatusCode ? Ok() : StatusCode(500);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            // URL: api/ProductsApi/Delete/5?tenantId=XYZ
            var url = $"api/ProductsApi/Delete/{id}?tenantId={CurrentTenantId}";
            var response = await _apiService.PostAsync<object>(url, null);

            return response.IsSuccessStatusCode ? Ok() : BadRequest();
        }
    }
}