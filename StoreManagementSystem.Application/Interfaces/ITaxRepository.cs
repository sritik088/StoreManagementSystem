using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ITaxRepository
    {
        Task<IEnumerable<Tax>> GetAllAsync();

        Task<Tax?> GetByIdAsync(int id);

        Task AddAsync(Tax tax);

        Task UpdateAsync(Tax tax);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(string name);

        Task<bool> ExistsAsync(string name, int id);
    }
}
