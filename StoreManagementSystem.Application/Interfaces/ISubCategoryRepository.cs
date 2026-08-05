using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISubCategoryRepository
    {
        Task<IEnumerable<SubCategory>> GetAllAsync();

        Task<SubCategory?> GetByIdAsync(int id);

        Task AddAsync(SubCategory subCategory);

        Task UpdateAsync(SubCategory subCategory);

        Task DeleteAsync(int id);

        Task<IEnumerable<SubCategory>> GetByCategoryAsync(int categoryId);
    }
}
