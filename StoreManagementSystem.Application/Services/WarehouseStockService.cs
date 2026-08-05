using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services;

public class WarehouseStockService : IWarehouseStockService
{
    private readonly IWarehouseStockRepository _repository;

    public WarehouseStockService(
        IWarehouseStockRepository repository)
    {
        _repository = repository;
    }

    public async Task IncreaseStockAsync(
    int warehouseId,
    int productId,
    decimal quantity)
    {
        var stock = await _repository.GetAsync(
            warehouseId,
            productId);

        if (stock == null)
        {
            stock = new WarehouseStock
            {
                WarehouseId = warehouseId,
                ProductId = productId,
                QuantityOnHand = quantity,
                ReservedQuantity = 0,
                LastUpdated = DateTime.Now
            };

            await _repository.AddAsync(stock);

            return;
        }

        stock.QuantityOnHand += quantity;
        stock.LastUpdated = DateTime.Now;

        _repository.Update(stock);
    }

    public async Task DecreaseStockAsync(
    int warehouseId,
    int productId,
    decimal quantity)
    {
        var stock = await _repository.GetAsync(
            warehouseId,
            productId);

        if (stock == null)
            throw new Exception("Stock not found.");

        if (stock.AvailableQuantity < quantity)
            throw new Exception("Insufficient stock.");

        stock.QuantityOnHand -= quantity;
        stock.LastUpdated = DateTime.Now;

        _repository.Update(stock);
    }

    public async Task ReserveStockAsync(
    int warehouseId,
    int productId,
    decimal quantity)
    {
        var stock = await _repository.GetAsync(
            warehouseId,
            productId);

        if (stock == null)
            throw new Exception("Stock not found.");

        if (stock.AvailableQuantity < quantity)
            throw new Exception("Insufficient stock.");

        stock.ReservedQuantity += quantity;
        stock.LastUpdated = DateTime.Now;

        _repository.Update(stock);
    }

    public async Task ReleaseReservedStockAsync(
    int warehouseId,
    int productId,
    decimal quantity)
    {
        var stock = await _repository.GetAsync(
            warehouseId,
            productId);

        if (stock == null)
            return;

        stock.ReservedQuantity -= quantity;

        if (stock.ReservedQuantity < 0)
            stock.ReservedQuantity = 0;

        stock.LastUpdated = DateTime.Now;

        _repository.Update(stock);
    }

    public async Task<decimal> GetAvailableStockAsync(
    int warehouseId,
    int productId)
    {
        var stock = await _repository.GetAsync(
            warehouseId,
            productId);

        return stock?.AvailableQuantity ?? 0;
    }

    public async Task<bool> HasStockAsync(
    int warehouseId,
    int productId,
    decimal quantity)
    {
        var available = await GetAvailableStockAsync(
            warehouseId,
            productId);

        return available >= quantity;
    }

}
