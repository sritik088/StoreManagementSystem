using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IMainCategoryService
    {
        Task<IEnumerable<MainCategory>> GetAllAsync();

        Task<MainCategory?> GetByIdAsync(int id);

        Task<bool> CreateAsync(MainCategory mainCategory);

        Task<bool> UpdateAsync(MainCategory mainCategory);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByNameAsync(string name);

        Task<bool> ExistsByNameAsync(string name, int id);
    }
}