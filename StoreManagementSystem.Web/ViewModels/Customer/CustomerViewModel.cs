
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Customer
{
    public class CustomerViewModel
    {
        public int Id { get; set; }

        // =========================
        // Basic Information
        // =========================

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(150)]
        [Display(Name = "Customer Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        // =========================
        // Contact Information
        // =========================

        [Required(ErrorMessage = "Mobile number is required.")]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string? Email { get; set; }

        // =========================
        // Address
        // =========================

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        // =========================
        // Tax Information
        // =========================

        [StringLength(20)]
        [Display(Name = "GST Number")]
        public string? GSTNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "PAN Number")]
        public string? PANNumber { get; set; }

        // =========================
        // Credit Information
        // =========================

        [Range(0, double.MaxValue, ErrorMessage = "Credit limit cannot be negative.")]
        [Display(Name = "Credit Limit")]
        public decimal CreditLimit { get; set; }

        [Range(0, 365, ErrorMessage = "Credit days must be between 0 and 365.")]
        [Display(Name = "Credit Days")]
        public int CreditDays { get; set; }

        // =========================
        // Status
        // =========================

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}


