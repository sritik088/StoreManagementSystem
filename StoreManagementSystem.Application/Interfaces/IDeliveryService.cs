using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IDeliveryService
    {
        Task<List<Delivery>> GetAllAsync();

        Task<Delivery?> GetByIdAsync(int id);

        Task<List<Delivery>> GetBySalesOrderIdAsync(
            int salesOrderId);

        Task<bool> CreateAsync(
            Delivery delivery);

        Task<decimal> GetDeliveredQuantityAsync(
            int salesOrderItemId);
    }
}