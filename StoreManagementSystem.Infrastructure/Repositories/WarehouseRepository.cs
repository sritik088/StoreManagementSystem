using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly ApplicationDbContext _context;

        public WarehouseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Warehouses
                .AnyAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name, int id)
        {
            return await _context.Warehouses
                .AnyAsync(x =>
                    x.Name == name &&
                    x.Id != id &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            
            
        }

        public async Task DeleteAsync(int id)
        {
            var warehouse = await GetByIdAsync(id);

            if (warehouse == null)
                return;

            warehouse.IsDeleted = true;
            warehouse.UpdatedDate = DateTime.Now;

            _context.Warehouses.Update(warehouse);

            await _context.SaveChangesAsync();
        }
    }
}
