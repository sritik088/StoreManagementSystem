using Microsoft.AspNetCore.Identity;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // =========================================================
    // WAREHOUSE ASSIGNMENT
    // =========================================================

    public int? WarehouseId { get; set; }

    public Warehouse? Warehouse { get; set; }
}