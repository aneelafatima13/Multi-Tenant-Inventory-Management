using Common;
using Common.Entities;
using DAL;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class UsersBAL
    {
        private readonly UsersDAL _usersDal;

        public UsersBAL(UsersDAL usersDal)
        {
            _usersDal = usersDal;
        }

        public async Task<bool> RegisterUserWithSubscriptionAsync(UserDto model, long currentUserId)
        {
            // 1. Map DTO to User Entity
            var user = new User
            {
                FullName = model.FullName,
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash), // Hash this before passing to BAL or here
                Role = model.Role,
                TenantId = model.TenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            var savedUser = await _usersDal.CreateUserAsync(user);

            // 2. Logic: If DurationMonths is provided, create the subscription
            if (model.DurationMonths > 0)
            {
                var subscription = new Subscription
                {
                    TenantId = model.TenantId,
                    UserId = savedUser.Id, // Linking to the new BigInt Id
                    ExpiryDate = DateTime.UtcNow.AddMonths(model.DurationMonths),
                    Status = 1 // Active
                };
                await _usersDal.CreateSubscriptionAsync(subscription);
            }

            return true;
        }


        public async Task<PagedResponse<UserListView>> GetUsersPagedAsync(int skip, int take, string search)
        {
            var result = await _usersDal.GetPagedUsersAsync(skip, take, search);
            return new PagedResponse<UserListView>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };
        }

        public async Task<UserDto?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _usersDal.GetUserForAuthAsync(username);

            // Verify user exists and password matches
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            // Return DTO with necessary info for the Token
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                TenantId = user.TenantId
            };
        }
    
    }
}