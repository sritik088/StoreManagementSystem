using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISidebarMenuService
    {
        Task<List<SidebarMenuSection>> GetSectionsAsync();

        Task SaveArrangementAsync(
            List<SidebarMenuSection> sections);

        Task SeedDefaultMenuAsync();
    }
}