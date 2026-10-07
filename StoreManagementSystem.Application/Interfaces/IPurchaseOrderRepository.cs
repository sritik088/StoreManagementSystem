using StoreManagementSystem.Domain.Entities;

public interface IPurchaseOrderRepository
{
    Task<IEnumerable<PurchaseOrder>> GetAllAsync();

    Task<PurchaseOrder?> GetByIdAsync(int id);

    Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id);

    Task<string> GeneratePONumberAsync();

    Task<decimal> GetLatestUnitPriceAsync(int productId);

    Task AddAsync(PurchaseOrder purchaseOrder);

    Task UpdateAsync(PurchaseOrder purchaseOrder);

    Task DeleteAsync(int id);

    Task RestoreAsync(int id);

    Task ApproveAsync(int id);

    Task CancelAsync(int id);

    Task<bool> HasActiveGoodsReceiptsAsync(int purchaseOrderId);
}