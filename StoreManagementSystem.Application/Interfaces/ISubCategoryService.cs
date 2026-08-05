using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISubCategoryService
    {
        Task<IEnumerable<SubCategory>> GetAllAsync();

        Task<SubCategory?> GetByIdAsync(int id);

        Task<bool> CreateAsync(SubCategory subCategory);

        Task<bool> UpdateAsync(SubCategory subCategory);

        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<SubCategory>> GetByCategoryAsync(int categoryId);
    }
}
