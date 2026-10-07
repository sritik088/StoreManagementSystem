using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IProductRepository
    {
        // =========================
        // GET ALL
        // =========================

        Task<IEnumerable<Product>> GetAllAsync();

        // =========================
        // GET BY ID
        // =========================

        Task<Product?> GetByIdAsync(int id);

        // =========================
        // CREATE
        // =========================

        Task AddAsync(Product product);

        // =========================
        // UPDATE
        // =========================

        Task UpdateAsync(Product product);

        // =========================
        // DELETE
        // =========================

        Task DeleteAsync(int id);

        // =========================
        // SKU VALIDATION
        // =========================

        Task<bool> ExistsBySkuAsync(string sku);

        Task<bool> ExistsBySkuAsync(
            string sku,
            int id);

        // =========================
        // BARCODE VALIDATION
        // =========================

        Task<bool> ExistsByBarcodeAsync(string barcode);

        Task<bool> ExistsByBarcodeAsync(
            string barcode,
            int id);
    }
}