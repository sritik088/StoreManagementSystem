using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public UnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync()
        {
            return await _context.Units
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _context.Units
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Units
                .AnyAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name, int id)
        {
            return await _context.Units
                .AnyAsync(x =>
                    x.Name == name &&
                    x.Id != id &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Unit unit)
        {
            await _context.Units.AddAsync(unit);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Unit unit)
        {
            _context.Units.Update(unit);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var unit = await GetByIdAsync(id);

            if (unit == null)
                return;

            unit.IsDeleted = true;
            unit.UpdatedDate = DateTime.Now;

            _context.Units.Update(unit);

            await _context.SaveChangesAsync();
        }
    }
}
