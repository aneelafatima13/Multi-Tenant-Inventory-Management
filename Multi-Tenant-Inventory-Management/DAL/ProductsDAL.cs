using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class ProductsDAL
    {
        private readonly ApplicationDbContext _context;

        public ProductsDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a paged list of products, optionally filtered by Name or SKU.
        /// </summary>
        public async Task<(List<Product> Items, int TotalCount)> GetPagedProductsAsync(int skip, int take, string search, string tenantId)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId) // Ensure multi-tenancy isolation
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdAsync(long id, string tenantId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        }

        public async Task<bool> SaveProductAsync(Product product)
        {
            try
            {
                
                // If Id is 0, it's a new record (assuming bigint Identity)
                if (product.Id == 0)
                {
                    product.Id = null; // Let the database generate the Id
                    product.CreatedDate = DateTime.UtcNow;
                    _context.Products.Add(product);
                }
                else
                {
                    product.ModifiedDate = DateTime.UtcNow;
                    _context.Products.Update(product);
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                // In production, log the exception (ex) here
                return false;
            }
        }

        /// <summary>
        /// Specifically updates stock levels efficiently.
        /// </summary>
        public async Task<bool> UpdateStockAsync(long id, int newStockLevel)
        {
            int rowsAffected = await _context.Products
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.StockLevel, newStockLevel)
                    .SetProperty(p => p.ModifiedDate, DateTime.UtcNow)
                );

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteProductAsync(long id, string tenantId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (product == null) return false;

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}