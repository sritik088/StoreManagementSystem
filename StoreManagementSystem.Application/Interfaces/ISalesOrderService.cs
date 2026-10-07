using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrder>> GetAllAsync();

        Task<SalesOrder?> GetByIdAsync(int id);

        Task<bool> CreateAsync(
            SalesOrder salesOrder);

        Task<bool> UpdateAsync(
            SalesOrder salesOrder);

        Task<bool> ConfirmAsync(
            int id);

        Task<bool> CancelAsync(
            int id);

        Task<bool> DeleteAsync(
            int id);

        Task<decimal?> GetLatestCostPriceAsync(
            int productId);
    }
}