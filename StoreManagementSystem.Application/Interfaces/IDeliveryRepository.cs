using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IDeliveryRepository
    {
        Task<List<Delivery>> GetAllAsync();

        Task<Delivery?> GetByIdAsync(int id);

        Task<List<Delivery>> GetBySalesOrderIdAsync(
            int salesOrderId);

        Task<decimal> GetDeliveredQuantityAsync(
            int salesOrderItemId);

        Task AddAsync(Delivery delivery);
    }
}