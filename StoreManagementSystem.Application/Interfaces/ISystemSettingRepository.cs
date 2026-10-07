using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISystemSettingRepository
    {
        Task<SystemSetting?> GetByKeyAsync(string key);

        Task AddAsync(SystemSetting setting);

        Task UpdateAsync(SystemSetting setting);
    }
}