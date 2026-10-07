using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Main Category
        [Required]
        public int MainCategoryId { get; set; }

        public MainCategory? MainCategory { get; set; }

        public bool IsDeleted { get; set; } = false;


        // Category → SubCategories
        public ICollection<SubCategory> SubCategories { get; set; }
            = new List<SubCategory>();

        // Category → Products
        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}