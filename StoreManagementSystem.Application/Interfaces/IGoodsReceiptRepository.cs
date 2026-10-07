using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IGoodsReceiptRepository
    {
        Task<IEnumerable<GoodsReceipt>> GetAllAsync();

        Task<GoodsReceipt?> GetByIdAsync(int id);

        Task<GoodsReceipt?> GetDeletedByIdAsync(int id);

        Task<PurchaseOrder?> GetPurchaseOrderWithDetailsAsync(int id);

        Task<string> GenerateGRNNumberAsync();

        Task<IEnumerable<PurchaseOrder>>
            GetApprovedPurchaseOrdersAsync();

        Task<decimal>
            GetPreviouslyReceivedQuantityAsync(
                int purchaseOrderItemId);

        Task AddAsync(GoodsReceipt receipt);

        Task UpdateAsync(GoodsReceipt receipt);

        Task DeleteAsync(int id);

        Task RestoreAsync(int id);

        Task RecalculatePurchaseOrderStatusAsync(
            int purchaseOrderId);
    }
}