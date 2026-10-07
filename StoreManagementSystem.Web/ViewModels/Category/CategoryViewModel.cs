using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Category
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a Main Category.")]
        [Display(Name = "Main Category")]
        public int MainCategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}