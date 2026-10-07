using StoreManagementSystem.Domain.Entities;

public interface IWarehouseStockService
{
    Task IncreaseStockAsync(
        int warehouseId,
        int productId,
        decimal quantity);

    Task DecreaseStockAsync(
        int warehouseId,
        int productId,
        decimal quantity);

    Task ReserveStockAsync(
        int warehouseId,
        int productId,
        decimal quantity);

    Task ReleaseReservedStockAsync(
        int warehouseId,
        int productId,
        decimal quantity);

    Task<decimal> GetAvailableStockAsync(
        int warehouseId,
        int productId);

    Task<bool> HasStockAsync(
        int warehouseId,
        int productId,
        decimal quantity);

    Task<List<WarehouseStock>> GetAllAsync();
}