using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Warehouse
{
    public class WarehouseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Warehouse Name is required.")]
        [Display(Name = "Warehouse Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Warehouse Code")]
        [StringLength(20)]
        public string? Code { get; set; }

        [Display(Name = "Manager Name")]
        [StringLength(100)]
        public string? ManagerName { get; set; }

        [Display(Name = "Contact Number")]
        [Phone(ErrorMessage = "Enter a valid contact number.")]
        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
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

        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(10)]
        public string? PostalCode { get; set; }

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
