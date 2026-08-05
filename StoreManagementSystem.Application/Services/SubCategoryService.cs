using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ISubCategoryRepository _repository;

        public SubCategoryService(ISubCategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SubCategory>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SubCategory?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<SubCategory>> GetByCategoryAsync(int categoryId)
        {
            return await _repository.GetByCategoryAsync(categoryId);
        }

        public async Task<bool> CreateAsync(SubCategory subCategory)
        {
            await _repository.AddAsync(subCategory);

            return true;
        }

        public async Task<bool> UpdateAsync(SubCategory subCategory)
        {
            await _repository.UpdateAsync(subCategory);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
