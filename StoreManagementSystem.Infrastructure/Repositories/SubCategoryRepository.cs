using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class SubCategoryRepository : ISubCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all SubCategories
        public async Task<IEnumerable<SubCategory>> GetAllAsync()
        {
            return await _context.SubCategories
                .Where(x => !x.IsDeleted)
                .Include(x => x.Category)
                .ThenInclude(x => x.MainCategory)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get SubCategory by ID
        public async Task<SubCategory?> GetByIdAsync(int id)
        {
            return await _context.SubCategories
                .Include(s => s.Category)
                    .ThenInclude(c => c.MainCategory)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // Get SubCategories by Category
        public async Task<IEnumerable<SubCategory>> GetByCategoryAsync(
            int categoryId)
        {
            return await _context.SubCategories
                .AsNoTracking()
                .Where(s => s.CategoryId == categoryId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        // Add SubCategory
        public async Task AddAsync(SubCategory subCategory)
        {
            await _context.SubCategories.AddAsync(subCategory);

            await _context.SaveChangesAsync();
        }

        // Update SubCategory
        public async Task UpdateAsync(SubCategory subCategory)
        {
            _context.SubCategories.Update(subCategory);

            await _context.SaveChangesAsync();
        }

        // Delete SubCategory
        public async Task DeleteAsync(int id)
        {
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subCategory == null)
                return;

            subCategory.IsDeleted = true;

            await _context.SaveChangesAsync();
        }
    }
}