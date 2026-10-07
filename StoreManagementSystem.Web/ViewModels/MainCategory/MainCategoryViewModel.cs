using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.MainCategory
{
    public class MainCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Main Category name is required.")]
        [StringLength(
            100,
            ErrorMessage = "Main Category name cannot exceed 100 characters.")]
        [Display(Name = "Main Category Name")]
        public string Name { get; set; } = string.Empty;
    }
}