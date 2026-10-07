using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces;

public interface IStockTransferService
{
    Task<List<StockTransfer>> GetAllAsync();

    Task<StockTransfer?> GetByIdAsync(int id);

    Task<bool> CreateAsync(StockTransfer transfer);

    Task<bool> CancelAsync(int id);

    Task<bool> DeleteAsync(int id);

    Task<bool> RestoreAsync(int id);
}