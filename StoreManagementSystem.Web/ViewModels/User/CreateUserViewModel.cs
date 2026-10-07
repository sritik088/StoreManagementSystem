using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.User;

public class CreateUserViewModel
{
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

    [DataType(DataType.Password)]
    [Required]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Required]
    [Compare(
        "Password",
        ErrorMessage = "Password and confirmation password do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Warehouse")]
    public int? WarehouseId { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(300)]
    public string? Address { get; set; }

    public IEnumerable<SelectListItem> Roles { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Warehouses { get; set; }
        = new List<SelectListItem>();
}