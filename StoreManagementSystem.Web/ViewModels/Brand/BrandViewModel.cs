using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Brand
{
    public class BrandViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand name is required.")]
        [Display(Name = "Brand Name")]
        [StringLength(100, ErrorMessage = "Maximum 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Brand Code")]
        [StringLength(20)]
        public string? Code { get; set; }

        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Website")]
        [Url(ErrorMessage = "Enter a valid website URL.")]
        [StringLength(200)]
        public string? Website { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;
    }
}
