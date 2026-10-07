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

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        // =========================
        // CREATE
        // =========================

        public async Task<bool> CreateAsync(Product product)
        {
            // Check duplicate SKU
            if (await _repository.ExistsBySkuAsync(product.SKU))
            {
                return false;
            }

            // Check duplicate Barcode
            if (!string.IsNullOrWhiteSpace(product.Barcode))
            {
                if (await _repository.ExistsByBarcodeAsync(product.Barcode))
                {
                    return false;
                }
            }

            // Clean input
            product.SKU = product.SKU.Trim();

            if (!string.IsNullOrWhiteSpace(product.Barcode))
            {
                product.Barcode = product.Barcode.Trim();
            }

            await _repository.AddAsync(product);

            return true;
        }

        // =========================
        // UPDATE
        // =========================

        public async Task<bool> UpdateAsync(Product product)
        {
            var existing =
                await _repository.GetByIdAsync(product.Id);

            if (existing == null)
            {
                return false;
            }

            // Check duplicate SKU
            if (await _repository.ExistsBySkuAsync(
                product.SKU,
                product.Id))
            {
                return false;
            }

            // Check duplicate Barcode
            if (!string.IsNullOrWhiteSpace(product.Barcode))
            {
                if (await _repository.ExistsByBarcodeAsync(
                    product.Barcode,
                    product.Id))
                {
                    return false;
                }
            }

            // =========================
            // Update Foreign Keys
            // =========================

            existing.MainCategoryId =
                product.MainCategoryId;

            existing.CategoryId =
                product.CategoryId;

            existing.SubCategoryId =
                product.SubCategoryId;

            existing.SupplierId =
                product.SupplierId;

            existing.WarehouseId =
                product.WarehouseId;

            // =========================
            // Update Identification
            // =========================

            existing.SKU =
                product.SKU.Trim();

            existing.Barcode =
                string.IsNullOrWhiteSpace(product.Barcode)
                    ? null
                    : product.Barcode.Trim();

            await _repository.UpdateAsync(existing);

            return true;
        }

        // =========================
        // DELETE
        // =========================

        public async Task<bool> DeleteAsync(int id)
        {
            var existing =
                await _repository.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}