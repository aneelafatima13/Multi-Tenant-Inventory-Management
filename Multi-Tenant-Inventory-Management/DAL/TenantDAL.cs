using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class TenantDAL
    {
        private readonly ApplicationDbContext _context;

        public TenantDAL(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            // We use AsNoTracking() for performance since this is a read-only list
            return await _context.Tenants
                .AsNoTracking()
                .ToListAsync();
        }
        

        public async Task<bool> SaveTenantAsync(Tenant tenant)
        {
            try
            {
                // 1. Check if ID is null or empty to determine if it's new
                var isNew = string.IsNullOrEmpty(tenant.Id);

                if (isNew)
                {
                    // FIX: Must await the Task to get the string value
                    tenant.Id = await GetUniqueShortIdAsync();
                    tenant.CreatedAt = DateTime.UtcNow;
                    _context.Tenants.Add(tenant);
                }
                else
                {
                    // For string IDs, check if it actually exists before updating
                    var exists = await _context.Tenants.AnyAsync(t => t.Id == tenant.Id);
                    if (!exists)
                    {
                        tenant.Id = await GetUniqueShortIdAsync();
                        tenant.ModifiedAt = DateTime.UtcNow;
                        _context.Tenants.Add(tenant);
                    }
                    else
                    {
                        _context.Tenants.Update(tenant);
                    }
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> GetUniqueShortIdAsync()
        {
            string newId;
            while (true)
            {
                newId = GenerateShortId();
                // Check uniqueness in DB
                if (!await _context.Tenants.AnyAsync(t => t.Id == newId))
                    break;
            }
            return newId;
        }

        private string GenerateShortId()
        {
            // Characters optimized for URL safety (removed symbols that break URLs like # or &)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // FIX: Changed parameter from Guid to string
        public async Task<Tenant?> GetByIdAsync(string id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task<(List<Tenant> Items, int TotalCount)> GetPagedTenantsAsync(int skip, int take, string search)
        {
            var query = _context.Tenants.AsNoTracking().AsQueryable(); // AsNoTracking for faster reads

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.BusinessName.Contains(search));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            
            return (items, totalCount);
        }

        public async Task<bool> UpdateStatusOnlyAsync(string id, int newStatus)
        {
            int rowsAffected = await _context.Tenants
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Status, newStatus)
                );

            return rowsAffected > 0;
        }

    }

      }