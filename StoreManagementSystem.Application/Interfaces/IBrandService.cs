using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllAsync();

        Task<Brand?> GetByIdAsync(int id);

        Task<IEnumerable<Brand>> SearchAsync(string keyword);

        Task<IEnumerable<Brand>> GetActiveAsync();

        Task<IEnumerable<Brand>> GetInactiveAsync();

        Task<int> CountAsync();

        Task<int> ActiveCountAsync();

        Task<int> InactiveCountAsync();

        Task<bool> CreateAsync(Brand brand);

        Task<bool> UpdateAsync(Brand brand);

        Task<bool> DeleteAsync(int id);
    }
}