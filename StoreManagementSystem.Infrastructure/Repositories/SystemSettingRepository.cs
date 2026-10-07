using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await _context.SystemSettings
                .FirstOrDefaultAsync(x => x.Key == key);
        }

        public async Task AddAsync(SystemSetting setting)
        {
            await _context.SystemSettings.AddAsync(setting);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SystemSetting setting)
        {
            _context.SystemSettings.Update(setting);

            await _context.SaveChangesAsync();
        }
    }
}