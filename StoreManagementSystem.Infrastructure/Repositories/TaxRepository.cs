using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class TaxRepository : ITaxRepository
    {
        private readonly ApplicationDbContext _context;

        public TaxRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tax>> GetAllAsync()
        {
            return await _context.Taxes
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.TaxPercentage)
                .ToListAsync();
        }

        public async Task<Tax?> GetByIdAsync(int id)
        {
            return await _context.Taxes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Taxes
                .AnyAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string name, int id)
        {
            return await _context.Taxes
                .AnyAsync(x =>
                    x.Name == name &&
                    x.Id != id &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Tax tax)
        {
            await _context.Taxes.AddAsync(tax);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tax tax)
        {
            _context.Taxes.Update(tax);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tax = await GetByIdAsync(id);

            if (tax == null)
                return;

            tax.IsDeleted = true;
            tax.UpdatedDate = DateTime.Now;

            _context.Taxes.Update(tax);

            await _context.SaveChangesAsync();
        }
    }
}
