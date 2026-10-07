using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class MainCategoryRepository : IMainCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public MainCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<MainCategory>> GetAllAsync()
        {
            return await _context.MainCategories
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        // =========================
        // GET BY ID
        // =========================

        public async Task<MainCategory?> GetByIdAsync(int id)
        {
            return await _context.MainCategories
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // =========================
        // CHECK DUPLICATE NAME
        // =========================

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.MainCategories
                .AnyAsync(x => x.Name == name);
        }

        // =========================
        // CHECK DUPLICATE NAME
        // EXCLUDING CURRENT RECORD
        // =========================

        public async Task<bool> ExistsByNameAsync(
            string name,
            int id)
        {
            return await _context.MainCategories
                .AnyAsync(x =>
                    x.Name == name &&
                    x.Id != id);
        }

        // =========================
        // ADD
        // =========================

        public async Task AddAsync(
            MainCategory mainCategory)
        {
            await _context.MainCategories
                .AddAsync(mainCategory);

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE
        // =========================

        public async Task UpdateAsync(
            MainCategory mainCategory)
        {
            _context.MainCategories
                .Update(mainCategory);

            await _context.SaveChangesAsync();
        }

        // =========================
        // DELETE
        // =========================

        public async Task DeleteAsync(int id)
        {
            var mainCategory = await _context.MainCategories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mainCategory == null)
                return;

            mainCategory.IsDeleted = true;

            await _context.SaveChangesAsync();
        }
    }
}