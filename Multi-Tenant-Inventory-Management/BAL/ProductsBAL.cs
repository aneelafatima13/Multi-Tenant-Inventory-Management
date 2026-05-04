using DAL;
using Common.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BAL
{
    public class ProductsBAL
    {
        private readonly ProductsDAL _pDal;

        public ProductsBAL(ProductsDAL pDal)
        {
            _pDal = pDal;
        }

        public async Task<(List<Product> Items, int TotalCount)> GetPagedProductsAsync(int page, int pageSize, string search, string tenantId)
        {
            // Calculate skip based on page number
            int skip = (page - 1) * pageSize;
            return await _pDal.GetPagedProductsAsync(skip, pageSize, search, tenantId);
        }

        public async Task<Product?> GetProductByIdAsync(long id, string tenantId)
        {
            if (id <= 0) return null;
            return await _pDal.GetByIdAsync(id, tenantId);
        }

        public async Task<bool> SaveProductAsync(Product product)
        {
            // Business Rule: Ensure SKU is uppercase
            if (!string.IsNullOrEmpty(product.SKU))
            {
                product.SKU = product.SKU.ToUpper().Trim();
            }

            // Business Rule: Basic validation before sending to DAL
            if (string.IsNullOrWhiteSpace(product.Name) || product.Price < 0)
            {
                return false;
            }

            return await _pDal.SaveProductAsync(product);
        }

        public async Task<bool> UpdateProductStockAsync(long id, int newStockLevel, string tenantId)
        {
            // Business Rule: Stock cannot be negative
            if (newStockLevel < 0) return false;

            // Security: Verify product belongs to tenant before updating
            var existing = await _pDal.GetByIdAsync(id, tenantId);
            if (existing == null) return false;

            return await _pDal.UpdateStockAsync(id, newStockLevel);
        }

        public async Task<bool> DeleteProductAsync(long id, string tenantId)
        {
            if (id <= 0) return false;
            return await _pDal.DeleteProductAsync(id, tenantId);
        }
    }
}