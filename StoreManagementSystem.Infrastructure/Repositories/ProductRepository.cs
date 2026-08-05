using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Brand)
                .Include(x => x.Supplier)
                .Include(x => x.Unit)
                .Include(x => x.Tax)
                .Include(x => x.Warehouse)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Brand)
                .Include(x => x.Supplier)
                .Include(x => x.Unit)
                .Include(x => x.Tax)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Products
                .AnyAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string name, int id)
        {
            return await _context.Products
                .AnyAsync(x =>
                    x.Name == name &&
                    x.Id != id &&
                    !x.IsDeleted);
        }

        public async Task<bool> ExistsBySkuAsync(string sku)
        {
            return await _context.Products
                .AnyAsync(x => x.SKU == sku && !x.IsDeleted);
        }

        public async Task<bool> ExistsBySkuAsync(string sku, int id)
        {
            return await _context.Products
                .AnyAsync(x =>
                    x.SKU == sku &&
                    x.Id != id &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);

            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);

            if (product == null)
                return;

            product.IsDeleted = true;
            product.UpdatedDate = DateTime.Now;

            _context.Products.Update(product);

            await _context.SaveChangesAsync();
        }
    }
}
