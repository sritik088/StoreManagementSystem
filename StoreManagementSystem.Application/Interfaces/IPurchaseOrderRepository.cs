using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();

        Task<PurchaseOrder?> GetByIdAsync(int id);

        Task AddAsync(PurchaseOrder purchaseOrder);

        Task UpdateAsync(PurchaseOrder purchaseOrder);

        Task DeleteAsync(int id);

        Task<string> GeneratePONumberAsync();

        Task ApproveAsync(int id);

        Task CancelAsync(int id);
    }
}
