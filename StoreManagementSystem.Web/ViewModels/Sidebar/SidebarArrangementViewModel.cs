using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Web.ViewModels.Sidebar
{
    public class SidebarArrangementViewModel
    {
        public List<SidebarMenuSection> Sections { get; set; }
            = new();
    }
}