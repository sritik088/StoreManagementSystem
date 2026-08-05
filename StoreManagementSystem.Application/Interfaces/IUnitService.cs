using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IUnitService
    {
        Task<IEnumerable<Unit>> GetAllAsync();

        Task<Unit?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Unit unit);

        Task<bool> UpdateAsync(Unit unit);

        Task<bool> DeleteAsync(int id);
    }
}
