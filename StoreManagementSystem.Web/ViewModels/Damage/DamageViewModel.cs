
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Damage
{
    public class DamageViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Damage code is required.")]
        [Display(Name = "Damage Code")]
        [StringLength(30, ErrorMessage = "Damage code cannot exceed 30 characters.")]
        public string DamageCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Damage name is required.")]
        [Display(Name = "Damage Name")]
        [StringLength(100, ErrorMessage = "Damage name cannot exceed 100 characters.")]
        public string DamageName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}

