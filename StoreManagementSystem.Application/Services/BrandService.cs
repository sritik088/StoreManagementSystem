using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repository;

        public BrandService(IBrandRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Brand>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await _repository.GetAllAsync();

            return await _repository.SearchAsync(keyword);
        }

        public async Task<IEnumerable<Brand>> GetActiveAsync()
        {
            return await _repository.GetActiveAsync();
        }

        public async Task<IEnumerable<Brand>> GetInactiveAsync()
        {
            return await _repository.GetInactiveAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _repository.CountAsync();
        }

        public async Task<int> ActiveCountAsync()
        {
            return await _repository.ActiveCountAsync();
        }

        public async Task<int> InactiveCountAsync()
        {
            return await _repository.InactiveCountAsync();
        }

        public async Task<bool> CreateAsync(Brand brand)
        {
            if (await _repository.ExistsAsync(brand.Name))
                return false;

            if (!string.IsNullOrWhiteSpace(brand.Code))
            {
                if (await _repository.CodeExistsAsync(brand.Code))
                    return false;
            }

            brand.CreatedDate = DateTime.Now;

            await _repository.AddAsync(brand);

            return true;
        }

        public async Task<bool> UpdateAsync(Brand brand)
        {
            var existing = await _repository.GetByIdAsync(brand.Id);

            if (existing == null)
                return false;

            existing.Name = brand.Name;
            existing.Code = brand.Code;
            existing.Country = brand.Country;
            existing.Website = brand.Website;
            existing.Description = brand.Description;
            existing.IsActive = brand.IsActive;
            existing.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(existing);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);

            return true;
        }
    }
}