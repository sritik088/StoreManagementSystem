using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IWarehouseStockRepository
    {
        Task<WarehouseStock?> GetAsync(int warehouseId, int productId);

        Task<List<WarehouseStock>> GetByProductAsync(int productId);

        Task<List<WarehouseStock>> GetByWarehouseAsync(int warehouseId);

        Task AddAsync(WarehouseStock stock);

        void Update(WarehouseStock stock);

        Task<bool> ExistsAsync(int warehouseId, int productId);
    }
}