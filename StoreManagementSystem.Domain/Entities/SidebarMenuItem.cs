using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class SidebarMenuItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Controller { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = "Index";

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(150)]
        public string? Permission { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public int SectionId { get; set; }

        public SidebarMenuSection? Section { get; set; }
    }
}