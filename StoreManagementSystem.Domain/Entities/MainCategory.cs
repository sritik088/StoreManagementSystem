using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class MainCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;


        // Main Category → Categories
        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        // Main Category → Products
        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}