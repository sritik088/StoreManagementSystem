using StoreManagementSystem.Domain.Entities;

public interface IWarehouseStockRepository
{
    Task<WarehouseStock?> GetAsync(
        int warehouseId,
        int productId);

    Task<List<WarehouseStock>> GetByProductAsync(
        int productId);

    Task<List<WarehouseStock>> GetByWarehouseAsync(
        int warehouseId);

    Task<List<WarehouseStock>> GetAllAsync();

    Task AddAsync(
        WarehouseStock stock);

    void Update(
        WarehouseStock stock);

    Task<bool> ExistsAsync(
        int warehouseId,
        int productId);
}