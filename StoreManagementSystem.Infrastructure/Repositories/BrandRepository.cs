using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<Brand?> GetByCodeAsync(string code)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(x => x.Code == code && !x.IsDeleted);
        }

        public async Task<IEnumerable<Brand>> SearchAsync(string keyword)
        {
            return await _context.Brands
                .Where(x =>
                    !x.IsDeleted &&
                    (x.Name.Contains(keyword) ||
                     (x.Code != null && x.Code.Contains(keyword)) ||
                     (x.Country != null && x.Country.Contains(keyword))))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Brand>> GetActiveAsync()
        {
            return await _context.Brands
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Brand>> GetInactiveAsync()
        {
            return await _context.Brands
                .Where(x => !x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Brands
                .AnyAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            return await _context.Brands
                .AnyAsync(x => x.Code == code && !x.IsDeleted);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Brands
                .CountAsync(x => !x.IsDeleted);
        }

        public async Task<int> ActiveCountAsync()
        {
            return await _context.Brands
                .CountAsync(x => x.IsActive && !x.IsDeleted);
        }

        public async Task<int> InactiveCountAsync()
        {
            return await _context.Brands
                .CountAsync(x => !x.IsActive && !x.IsDeleted);
        }

        public async Task AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Brand brand)
        {
            brand.UpdatedDate = DateTime.Now;

            _context.Brands.Update(brand);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await GetByIdAsync(id);

            if (brand == null)
                return;

            // Soft Delete
            brand.IsDeleted = true;
            brand.UpdatedDate = DateTime.Now;

            _context.Brands.Update(brand);

            await _context.SaveChangesAsync();
        }
    }
}