using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IGoodsReceiptService
    {
        Task<IEnumerable<GoodsReceipt>> GetAllAsync();

        Task<GoodsReceipt?> GetByIdAsync(int id);

        Task<bool> CreateAsync(GoodsReceipt receipt);

        Task<bool> DeleteAsync(int id);
    }
}