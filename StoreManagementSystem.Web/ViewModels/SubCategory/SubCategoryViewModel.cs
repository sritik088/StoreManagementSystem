using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.SubCategory
{
    public class SubCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category name is required.")]
        [Display(Name = "Sub Category")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
