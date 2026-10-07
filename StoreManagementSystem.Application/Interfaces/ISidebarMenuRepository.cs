using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISidebarMenuRepository
    {
        Task<List<SidebarMenuSection>> GetSectionsAsync();

        Task<bool> AnyAsync();

        Task SaveArrangementAsync(
            List<SidebarMenuSection> sections);

        Task AddSectionsAsync(
            List<SidebarMenuSection> sections);
    }
}