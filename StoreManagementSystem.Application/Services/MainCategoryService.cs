using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class MainCategoryService : IMainCategoryService
    {
        private readonly IMainCategoryRepository _repository;

        public MainCategoryService(
            IMainCategoryRepository repository)
        {
            _repository = repository;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<MainCategory>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<MainCategory?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _repository.GetByIdAsync(id);
        }

        // =========================
        // CREATE
        // =========================

        public async Task<bool> CreateAsync(
            MainCategory mainCategory)
        {
            if (mainCategory == null)
                return false;

            if (string.IsNullOrWhiteSpace(mainCategory.Name))
                return false;

            mainCategory.Name =
                mainCategory.Name.Trim();

            // Check duplicate name
            if (await _repository.ExistsByNameAsync(
                    mainCategory.Name))
            {
                return false;
            }

            await _repository.AddAsync(mainCategory);

            return true;
        }

        // =========================
        // UPDATE
        // =========================

        public async Task<bool> UpdateAsync(
            MainCategory mainCategory)
        {
            if (mainCategory == null)
                return false;

            if (mainCategory.Id <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(mainCategory.Name))
                return false;

            var existing =
                await _repository.GetByIdAsync(
                    mainCategory.Id);

            if (existing == null)
                return false;

            mainCategory.Name =
                mainCategory.Name.Trim();

            // Check duplicate name
            if (await _repository.ExistsByNameAsync(
                    mainCategory.Name,
                    mainCategory.Id))
            {
                return false;
            }

            existing.Name =
                mainCategory.Name;

            await _repository.UpdateAsync(existing);

            return true;
        }

        // =========================
        // DELETE
        // =========================

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                return false;

            var existing =
                await _repository.GetByIdAsync(id);

            if (existing == null)
                return false;

            // Don't delete MainCategory
            // if Categories exist
            if (existing.Categories.Any())
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }

        // =========================
        // EXISTS
        // =========================

        public async Task<bool> ExistsByNameAsync(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _repository.ExistsByNameAsync(
                name.Trim());
        }

        public async Task<bool> ExistsByNameAsync(
            string name,
            int id)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _repository.ExistsByNameAsync(
                name.Trim(),
                id);
        }
    }
}