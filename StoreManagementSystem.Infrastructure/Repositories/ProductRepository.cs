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

        // =========================
        // GET ALL PRODUCTS
        // =========================

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(x => x.MainCategory)
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Supplier)
                .Include(x => x.Warehouse)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(x => x.MainCategory)
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.Supplier)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        // =========================
        // CHECK DUPLICATE SKU
        // =========================

        public async Task<bool> ExistsBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            sku = sku.Trim();

            return await _context.Products
                .AnyAsync(x => x.SKU == sku);
        }

        // =========================
        // CHECK DUPLICATE SKU
        // EXCLUDE CURRENT PRODUCT
        // =========================

        public async Task<bool> ExistsBySkuAsync(
            string sku,
            int id)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            sku = sku.Trim();

            return await _context.Products
                .AnyAsync(x =>
                    x.SKU == sku &&
                    x.Id != id);
        }

        // =========================
        // CHECK DUPLICATE BARCODE
        // =========================

        public async Task<bool> ExistsByBarcodeAsync(
            string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            barcode = barcode.Trim();

            return await _context.Products
                .AnyAsync(x => x.Barcode == barcode);
        }

        // =========================
        // CHECK DUPLICATE BARCODE
        // EXCLUDE CURRENT PRODUCT
        // =========================

        public async Task<bool> ExistsByBarcodeAsync(
            string barcode,
            int id)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            barcode = barcode.Trim();

            return await _context.Products
                .AnyAsync(x =>
                    x.Barcode == barcode &&
                    x.Id != id);
        }

        // =========================
        // ADD PRODUCT
        // =========================

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE PRODUCT
        // =========================

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);

            await _context.SaveChangesAsync();
        }

        // =========================
        // DELETE PRODUCT
        // =========================

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return;

            product.IsDeleted = true;

            await _context.SaveChangesAsync();
        }
    }
}