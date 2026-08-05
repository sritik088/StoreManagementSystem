using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IUnitRepository
    {
        Task<IEnumerable<Unit>> GetAllAsync();

        Task<Unit?> GetByIdAsync(int id);

        Task AddAsync(Unit unit);

        Task UpdateAsync(Unit unit);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(string name);

        Task<bool> ExistsAsync(string name, int id);
    }
}
