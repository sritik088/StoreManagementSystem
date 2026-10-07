using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrder>> GetAllAsync();

        Task<SalesOrder?> GetByIdAsync(int id);

        Task<SalesOrder?> GetByIdWithDetailsAsync(int id);

        Task<SalesOrder> AddAsync(SalesOrder salesOrder);

        Task UpdateAsync(SalesOrder salesOrder);

        Task<bool> ExistsAsync(int id);

        Task<decimal?> GetLatestCostPriceAsync(int productId);
    }
}