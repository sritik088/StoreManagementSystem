using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IWarehouseService
    {
        Task<IEnumerable<Warehouse>> GetAllAsync();

        Task<Warehouse?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Warehouse warehouse);

        Task<bool> UpdateAsync(Warehouse warehouse);

        Task<bool> DeleteAsync(int id);
    }
}
