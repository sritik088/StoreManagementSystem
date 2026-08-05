using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();

        Task<PurchaseOrder?> GetByIdAsync(int id);

        Task<bool> CreateAsync(PurchaseOrder purchaseOrder);

        Task<bool> UpdateAsync(PurchaseOrder purchaseOrder);

        Task<bool> DeleteAsync(int id);

        Task<bool> ApproveAsync(int id);

        Task<bool> CancelAsync(int id);
    }
}
