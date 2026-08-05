using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class TaxService : ITaxService
    {
        private readonly ITaxRepository _repository;

        public TaxService(ITaxRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Tax>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Tax?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Tax tax)
        {
            if (await _repository.ExistsAsync(tax.Name))
                return false;

            tax.CreatedDate = DateTime.Now;
            tax.IsActive = true;
            tax.IsDeleted = false;

            await _repository.AddAsync(tax);

            return true;
        }

        public async Task<bool> UpdateAsync(Tax tax)
        {
            if (await _repository.ExistsAsync(tax.Name, tax.Id))
                return false;

            var existing = await _repository.GetByIdAsync(tax.Id);

            if (existing == null)
                return false;

            existing.Name = tax.Name;
            existing.TaxPercentage = tax.TaxPercentage;
            existing.TaxType = tax.TaxType;
            existing.Description = tax.Description;
            existing.IsActive = tax.IsActive;
            existing.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(existing);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
