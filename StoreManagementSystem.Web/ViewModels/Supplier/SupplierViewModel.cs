using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StoreManagementSystem.Web.ViewModels.Supplier
{
    public class SupplierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Supplier name is required.")]
        [Display(Name = "Supplier Name")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Supplier Code")]
        [StringLength(20)]
        public string? SupplierCode { get; set; }

        [Required(ErrorMessage = "Contact person is required.")]
        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [Display(Name = "Phone Number")]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email Address")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Address")]
        [StringLength(300)]
        public string? Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string? City { get; set; }

        [Display(Name = "State")]
        [StringLength(100)]
        public string? State { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "GST Number")]
        [StringLength(20)]
        public string? GSTNumber { get; set; }

        [Display(Name = "Website")]
        [Url]
        [StringLength(100)]
        public string? Website { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Supplier Logo")]
        public IFormFile? Logo { get; set; }

        public string? ExistingLogo { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
