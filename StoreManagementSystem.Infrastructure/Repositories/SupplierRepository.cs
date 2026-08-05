using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<Supplier>> SearchAsync(string search)
        {
            search = search.ToLower();

            return await _context.Suppliers
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.Name.ToLower().Contains(search) ||
                        (x.SupplierCode != null &&
                         x.SupplierCode.ToLower().Contains(search)) ||
                        (x.ContactPerson != null &&
                         x.ContactPerson.ToLower().Contains(search))
                    ))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Suppliers
                .AnyAsync(x =>
                    x.Name == name &&
                    !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name, int id)
        {
            return await _context.Suppliers
                .AnyAsync(x =>
                    x.Name == name &&
                    x.Id != id &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);

            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await GetByIdAsync(id);

            if (supplier == null)
                return;

            supplier.IsDeleted = true;
            supplier.UpdatedDate = DateTime.Now;

            _context.Suppliers.Update(supplier);

            await _context.SaveChangesAsync();
        }
    }
}