using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.User;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First Name")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Role")]
    [Required]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Warehouse")]
    public int? WarehouseId { get; set; }

    public bool IsActive { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public IEnumerable<SelectListItem> Roles { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Warehouses { get; set; }
        = new List<SelectListItem>();
}