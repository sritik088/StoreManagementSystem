using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _repository;

        public WarehouseService(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Warehouse warehouse)
        {
            if (await _repository.ExistsAsync(warehouse.Name))
                return false;

            warehouse.CreatedDate = DateTime.Now;
            warehouse.IsActive = true;
            warehouse.IsDeleted = false;

            await _repository.AddAsync(warehouse);

            return true;
        }

        public async Task<bool> UpdateAsync(Warehouse warehouse)
        {
            if (await _repository.ExistsAsync(warehouse.Name, warehouse.Id))
                return false;

            var existingWarehouse = await _repository.GetByIdAsync(warehouse.Id);

            if (existingWarehouse == null)
                return false;

            existingWarehouse.Name = warehouse.Name;
            existingWarehouse.Code = warehouse.Code;
            existingWarehouse.ManagerName = warehouse.ManagerName;
            existingWarehouse.ContactNumber = warehouse.ContactNumber;
            existingWarehouse.Email = warehouse.Email;
            existingWarehouse.Address = warehouse.Address;
            existingWarehouse.City = warehouse.City;
            existingWarehouse.State = warehouse.State;
            existingWarehouse.Country = warehouse.Country;
            existingWarehouse.PostalCode = warehouse.PostalCode;
            existingWarehouse.Description = warehouse.Description;
            existingWarehouse.IsActive = warehouse.IsActive;
            existingWarehouse.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(existingWarehouse);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warehouse = await _repository.GetByIdAsync(id);

            if (warehouse == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}