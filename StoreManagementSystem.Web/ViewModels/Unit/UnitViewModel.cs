using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Unit
{
    public class UnitViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Unit Name is required.")]
        [Display(Name = "Unit Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Short Name")]
        [StringLength(20)]
        public string? ShortName { get; set; }

        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
