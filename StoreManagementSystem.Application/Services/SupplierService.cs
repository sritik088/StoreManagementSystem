using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Supplier>> SearchAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return await _repository.GetAllAsync();

            return await _repository.SearchAsync(search);
        }

        public async Task<bool> CreateAsync(Supplier supplier)
        {
            if (await _repository.ExistsAsync(supplier.Name))
                return false;

            supplier.CreatedDate = DateTime.Now;
            supplier.IsActive = true;

            await _repository.AddAsync(supplier);

            return true;
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            if (await _repository.ExistsAsync(supplier.Name, supplier.Id))
                return false;

            supplier.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(supplier);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
