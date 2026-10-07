using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class SidebarMenuSection
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<SidebarMenuItem> Items { get; set; }
            = new List<SidebarMenuItem>();
    }
}