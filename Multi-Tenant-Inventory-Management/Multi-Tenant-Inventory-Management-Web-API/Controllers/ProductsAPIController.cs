using BAL;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Multi_Tenant_Inventory_Management_Web_API.Controllers
{
    [Authorize] // 1. Secure the entire controller
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ProductsBAL _productsBal;

        public ProductsApiController(ProductsBAL productsBal)
        {
            _productsBal = productsBal;
        }

        // 2. Helper properties to extract data from JWT safely
        private string CurrentTenantId => User.FindFirstValue("TenantId");
        private long CurrentUserId => long.Parse(User.FindFirstValue("UserId") ?? "0");

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10, string? search = "")
        {
            // We ignore any tenantId passed in the URL and use the one from the Token
            var result = await _productsBal.GetPagedProductsAsync(page, pageSize, search ?? "", CurrentTenantId);
            return Ok(new { result.Items, result.TotalCount });
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Post([FromBody] Product model)
        {
            try
            {
                // FORCE the TenantId and CreatedBy from the Token
                model.TenantId = CurrentTenantId;
                
                if (model.Id == 0)
                {
                    model.CreatedbyId = CurrentUserId;
                }
                else
                {
                    model.ModifiedbyId = CurrentUserId;
                }

                var success = await _productsBal.SaveProductAsync(model);
                return success ? Ok(new { message = "Saved successfully" }) : BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPatch("UpdateStock/{id}")]
        public async Task<IActionResult> UpdateStock(long id, [FromQuery] int newStock)
        {
            // Security: CurrentTenantId ensures they can only update THEIR product
            var success = await _productsBal.UpdateProductStockAsync(id, newStock, CurrentTenantId);
            return success ? Ok() : Unauthorized();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id, string tenantId)
        {
            var success = await _productsBal.DeleteProductAsync(id, tenantId);
            return success ? Ok(new { message = "Product deleted" }) : NotFound();
        }
    }
}