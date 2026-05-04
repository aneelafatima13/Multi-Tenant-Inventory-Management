using BAL;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Multi_Tenant_Inventory_Management_Web_API.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class LoginAuthAPIController : ControllerBase
    {
        private readonly UsersBAL _usersBal;
        private readonly IConfiguration _configuration;

        public LoginAuthAPIController(UsersBAL usersBal, IConfiguration configuration)
        {
            _usersBal = usersBal;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // 1. Ask BAL to verify credentials
            var authUser = await _usersBal.AuthenticateUserAsync(model.Email, model.Password);

            if (authUser == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // 2. Generate JWT Token (Controller logic)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, authUser.Username),
                new Claim(ClaimTypes.Role, authUser.Role),
                new Claim("TenantId", authUser.TenantId),
                new Claim("UserId", authUser.Id.ToString())
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new ApiResponse // Use the class to ensure consistency
            {
                Token = tokenHandler.WriteToken(token),
                Role = authUser.Role,
                UserId = authUser.Id.ToString(),
                TenantId = authUser.TenantId
            });
        }
    }
}
