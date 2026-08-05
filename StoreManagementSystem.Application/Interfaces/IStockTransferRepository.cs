using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces;

public interface IStockTransferRepository
{
    Task<List<StockTransfer>> GetAllAsync();

    Task<StockTransfer?> GetByIdAsync(int id);

    Task AddAsync(StockTransfer transfer);

    void Update(StockTransfer transfer);

    Task<bool> ExistsAsync(int id);

    Task<string> GenerateTransferNumberAsync();
}