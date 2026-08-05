using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<Warehouse>> GetAllAsync();

        Task<Warehouse?> GetByIdAsync(int id);

        Task AddAsync(Warehouse warehouse);

        Task UpdateAsync(Warehouse warehouse);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(string name);

        Task<bool> ExistsAsync(string name, int id);
    }
}
