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
            if (id <= 0)
                return null;

            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<SubCategory>> GetByCategoryAsync(
            int categoryId)
        {
            if (categoryId <= 0)
                return Enumerable.Empty<SubCategory>();

            return await _repository.GetByCategoryAsync(categoryId);
        }

        public async Task<bool> CreateAsync(SubCategory subCategory)
        {
            if (subCategory == null)
                return false;

            if (subCategory.CategoryId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(subCategory.Name))
                return false;

            subCategory.Name = subCategory.Name.Trim();

            await _repository.AddAsync(subCategory);

            return true;
        }

        public async Task<bool> UpdateAsync(SubCategory subCategory)
        {
            if (subCategory == null)
                return false;

            if (subCategory.Id <= 0)
                return false;

            if (subCategory.CategoryId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(subCategory.Name))
                return false;

            var existingSubCategory =
                await _repository.GetByIdAsync(subCategory.Id);

            if (existingSubCategory == null)
                return false;

            existingSubCategory.CategoryId = subCategory.CategoryId;
            existingSubCategory.Name = subCategory.Name.Trim();

            await _repository.UpdateAsync(existingSubCategory);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                return false;

            var existingSubCategory =
                await _repository.GetByIdAsync(id);

            if (existingSubCategory == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}