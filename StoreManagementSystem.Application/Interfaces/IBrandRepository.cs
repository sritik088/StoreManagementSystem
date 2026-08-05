using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllAsync();

        Task<Brand?> GetByIdAsync(int id);

        Task<Brand?> GetByCodeAsync(string code);

        Task<IEnumerable<Brand>> SearchAsync(string keyword);

        Task<IEnumerable<Brand>> GetActiveAsync();

        Task<IEnumerable<Brand>> GetInactiveAsync();

        Task<bool> ExistsAsync(string name);

        Task<bool> CodeExistsAsync(string code);

        Task<int> CountAsync();

        Task<int> ActiveCountAsync();

        Task<int> InactiveCountAsync();

        Task AddAsync(Brand brand);

        Task UpdateAsync(Brand brand);

        Task DeleteAsync(int id);
    }
}