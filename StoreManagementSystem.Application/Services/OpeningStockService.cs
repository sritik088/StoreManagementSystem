using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Application.Services;
using StoreManagementSystem.Domain.Enums;

public class OpeningStockService : IOpeningStockService
{
    private readonly IWarehouseStockRepository _warehouseStockRepository;
    private readonly IStockLedgerService _stockLedgerService;

    public OpeningStockService(
        IWarehouseStockRepository warehouseStockRepository,
        IStockLedgerService stockLedgerService)
    {
        _warehouseStockRepository = warehouseStockRepository;
        _stockLedgerService = stockLedgerService;
    }

    public async Task AddOpeningStockAsync(
        int warehouseId,
        int productId,
        decimal openingQuantity,
        decimal purchasePrice)
    {
        if (openingQuantity <= 0)
            throw new InvalidOperationException(
                "Opening stock must be greater than zero.");


        await _stockLedgerService
            .AddStockMovementAsync(
                productId,
                warehouseId,
                openingQuantity,
                StockTransactionType.OpeningStock,
                purchasePrice,
                $"OPEN-{productId}",
                "Opening stock");
    }
}