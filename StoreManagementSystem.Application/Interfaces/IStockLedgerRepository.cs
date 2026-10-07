using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IStockLedgerRepository
    {
        Task AddAsync(StockLedger ledger);

        Task<IEnumerable<StockLedger>> GetAllAsync();

        Task<IEnumerable<StockLedger>> GetByProductAsync(
            int productId);

        Task<IEnumerable<StockLedger>> GetByWarehouseAsync(
            int warehouseId);

        Task<IEnumerable<StockLedger>> GetByProductAndWarehouseAsync(
            int productId,
            int warehouseId);

        Task<decimal> GetCurrentBalanceAsync(
            int productId,
            int warehouseId);

        Task<StockLedger?> GetLastEntryAsync(
            int productId,
            int warehouseId);
    }
}