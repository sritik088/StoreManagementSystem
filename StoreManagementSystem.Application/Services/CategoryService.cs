using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<Category?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _repository.GetByIdAsync(id);
        }

        // =========================
        // CREATE
        // =========================

        public async Task<bool> CreateAsync(Category category)
        {
            if (category == null)
                return false;

            if (string.IsNullOrWhiteSpace(category.Name))
                return false;

            if (category.MainCategoryId <= 0)
                return false;

            category.Name = category.Name.Trim();

            if (await _repository.ExistsAsync(category.Name))
                return false;

            await _repository.AddAsync(category);

            return true;
        }

        // =========================
        // UPDATE
        // =========================

        public async Task<bool> UpdateAsync(Category category)
        {
            if (category == null)
                return false;

            if (category.Id <= 0)
                return false;

            if (category.MainCategoryId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(category.Name))
                return false;

            var existingCategory =
                await _repository.GetByIdAsync(category.Id);

            if (existingCategory == null)
                return false;

            category.Name = category.Name.Trim();

            await _repository.UpdateAsync(category);

            return true;
        }

        // =========================
        // DELETE
        // =========================

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                return false;

            var category =
                await _repository.GetByIdAsync(id);

            if (category == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}