using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IProductService
    {
        // =========================
        // GET ALL PRODUCTS
        // =========================

        Task<IEnumerable<Product>> GetAllAsync();

        // =========================
        // GET PRODUCT BY ID
        // =========================

        Task<Product?> GetByIdAsync(int id);

        // =========================
        // CREATE PRODUCT
        // =========================

        Task<bool> CreateAsync(Product product);

        // =========================
        // UPDATE PRODUCT
        // =========================

        Task<bool> UpdateAsync(Product product);

        // =========================
        // DELETE PRODUCT
        // =========================

        Task<bool> DeleteAsync(int id);
    }
}