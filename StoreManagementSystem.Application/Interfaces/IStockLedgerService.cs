using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IStockLedgerService
    {
        Task AddStockMovementAsync(
            int productId,
            int warehouseId,
            decimal quantity,
            StockTransactionType transactionType,
            decimal unitCost,
            string referenceNo,
            string? remarks = null,
            string? createdBy = null);
    }
}