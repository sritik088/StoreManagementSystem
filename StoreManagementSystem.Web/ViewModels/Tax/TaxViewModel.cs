using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Tax
{
    public class TaxViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tax Name is required.")]
        [Display(Name = "Tax Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax Percentage is required.")]
        [Display(Name = "Tax Percentage (%)")]
        [Range(0, 100,
            ErrorMessage = "Tax Percentage must be between 0 and 100.")]
        public decimal TaxPercentage { get; set; }

        [Required(ErrorMessage = "Tax Type is required.")]
        [Display(Name = "Tax Type")]
        [StringLength(100)]
        public string TaxType { get; set; } = "GST";

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
