using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Navigation Property
        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}
