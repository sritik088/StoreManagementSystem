using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ITaxService
    {
        Task<IEnumerable<Tax>> GetAllAsync();

        Task<Tax?> GetByIdAsync(int id);

        Task<bool> CreateAsync(Tax tax);

        Task<bool> UpdateAsync(Tax tax);

        Task<bool> DeleteAsync(int id);
    }
}
