using BAL;
using Common;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Multi_Tenant_Inventory_Management_Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly UsersBAL _usersBal;

        public UserAPIController(UsersBAL usersBal)
        {
            _usersBal = usersBal;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto model)
        {
            try
            {
                long currentUserId = 1;
                var result = await _usersBal.RegisterUserWithSubscriptionAsync(model, currentUserId);

                if (result) return Ok();

                return BadRequest("The system was unable to register the user. Please check your inputs.");
            }
            catch (Exception ex)
            {
                // Return the actual inner exception message for debugging
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("paged")]
        // Ensure the parameter names match what GetPagedAsync sends (usually skip, take, search)
        public async Task<IActionResult> GetPaged([FromQuery] int skip, [FromQuery] int take, [FromQuery] string? search = "")
        {
            var result = await _usersBal.GetUsersPagedAsync(skip, take, search ?? "");
            return Ok(result);
        }

        
    }
}