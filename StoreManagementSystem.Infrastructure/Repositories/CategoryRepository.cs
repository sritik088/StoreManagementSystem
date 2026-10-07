using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(x => !x.IsDeleted)
                .Include(x => x.MainCategory)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.MainCategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // =========================
        // CHECK DUPLICATE
        // =========================

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name == name);
        }

        // =========================
        // ADD
        // =========================

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE
        // =========================

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);

            await _context.SaveChangesAsync();
        }

        // =========================
        // DELETE
        // =========================

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return;

            category.IsDeleted = true;

            await _context.SaveChangesAsync();
        }
    }
}