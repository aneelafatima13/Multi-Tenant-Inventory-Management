using Common;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class UsersDAL
    {
        private readonly ApplicationDbContext _context;

        public UsersDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user; // Return the user so BAL can get the generated BigInt Id
        }

        public async Task<bool> CreateSubscriptionAsync(Subscription sub)
        {
            _context.Subscriptions.Add(sub);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<(List<UserListView> Items, int TotalCount)> GetPagedUsersAsync(int skip, int take, string search)
        {
            var query = _context.UserListView.AsNoTracking().IgnoreQueryFilters().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(v => v.Username.Contains(search) ||
                                         v.BusinessName.Contains(search));
            }

            int total = await query.CountAsync();
            var items = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip(skip).Take(take)
                .ToListAsync();

            return (items, total);
        }

        public async Task<User?> GetUserForAuthAsync(string username)
        {
            // We bypass filters because we don't know the TenantId yet
            return await _context.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}

