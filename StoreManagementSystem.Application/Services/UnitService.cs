using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _repository;

        public UnitService(IUnitRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Unit unit)
        {
            if (await _repository.ExistsAsync(unit.Name))
                return false;

            unit.CreatedDate = DateTime.Now;
            unit.IsActive = true;
            unit.IsDeleted = false;

            await _repository.AddAsync(unit);

            return true;
        }

        public async Task<bool> UpdateAsync(Unit unit)
        {
            if (await _repository.ExistsAsync(unit.Name, unit.Id))
                return false;

            var existing = await _repository.GetByIdAsync(unit.Id);

            if (existing == null)
                return false;

            existing.Name = unit.Name;
            existing.ShortName = unit.ShortName;
            existing.Description = unit.Description;
            existing.IsActive = unit.IsActive;
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
