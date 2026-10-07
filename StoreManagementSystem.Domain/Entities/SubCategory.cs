using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementSystem.Domain.Entities
{
    public class SubCategory
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;


        public ICollection<Product> Products { get; set; }
        = new List<Product>();
    }
}
