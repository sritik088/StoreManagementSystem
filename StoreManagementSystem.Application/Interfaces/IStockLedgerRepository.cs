using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IStockLedgerRepository
    {
        Task AddAsync(StockLedger ledger);

        Task<IEnumerable<StockLedger>> GetByProductAsync(int productId);
    }
}