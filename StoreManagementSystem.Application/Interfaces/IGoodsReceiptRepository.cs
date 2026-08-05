using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IGoodsReceiptRepository
    {
        Task<IEnumerable<GoodsReceipt>> GetAllAsync();

        Task<GoodsReceipt?> GetByIdAsync(int id);

        Task AddAsync(GoodsReceipt receipt);

        Task UpdateAsync(GoodsReceipt receipt);

        Task DeleteAsync(int id);

        Task<string> GenerateGRNNumberAsync();

        Task<IEnumerable<PurchaseOrder>> GetApprovedPurchaseOrdersAsync();
    }
}