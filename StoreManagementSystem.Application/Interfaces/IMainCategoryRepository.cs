using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface IMainCategoryRepository
    {
        Task<IEnumerable<MainCategory>> GetAllAsync();

        Task<MainCategory?> GetByIdAsync(int id);

        Task AddAsync(MainCategory mainCategory);

        Task UpdateAsync(MainCategory mainCategory);

        Task DeleteAsync(int id);

        Task<bool> ExistsByNameAsync(string name);

        Task<bool> ExistsByNameAsync(string name, int id);
    }
}