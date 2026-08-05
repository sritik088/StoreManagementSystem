using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Product product)
        {
            // Duplicate Product Name
            if (await _repository.ExistsByNameAsync(product.Name))
                return false;

            // Duplicate SKU
            if (await _repository.ExistsBySkuAsync(product.SKU))
                return false;

            product.CreatedDate = DateTime.Now;
            product.IsActive = true;
            product.IsDeleted = false;

            // Initial Stock
            product.CurrentStock = product.OpeningStock;

            await _repository.AddAsync(product);

            return true;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            // Duplicate Product Name
            if (await _repository.ExistsByNameAsync(product.Name, product.Id))
                return false;

            // Duplicate SKU
            if (await _repository.ExistsBySkuAsync(product.SKU, product.Id))
                return false;

            var existing = await _repository.GetByIdAsync(product.Id);

            if (existing == null)
                return false;

            // Basic Information
            existing.Name = product.Name;
            existing.SKU = product.SKU;
            existing.Barcode = product.Barcode;
            existing.HSNCode = product.HSNCode;

            // Relationships
            existing.CategoryId = product.CategoryId;
            existing.SubCategoryId = product.SubCategoryId;
            existing.BrandId = product.BrandId;
            existing.SupplierId = product.SupplierId;
            existing.UnitId = product.UnitId;
            existing.TaxId = product.TaxId;
            existing.WarehouseId = product.WarehouseId;

            // Pricing
            existing.PurchasePrice = product.PurchasePrice;
            existing.SellingPrice = product.SellingPrice;
            existing.DiscountPrice = product.DiscountPrice;

            // Inventory
            existing.OpeningStock = product.OpeningStock;
            existing.CurrentStock = product.CurrentStock;
            existing.ReorderLevel = product.ReorderLevel;
            existing.MaximumStock = product.MaximumStock;

            // Other Details
            existing.ImageUrl = product.ImageUrl;
            existing.Description = product.Description;
            existing.IsActive = product.IsActive;
            existing.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(existing);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
