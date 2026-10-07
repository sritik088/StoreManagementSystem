namespace StoreManagementSystem.Web.ViewModels.User;

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName =>
        $"{FirstName} {LastName}".Trim();

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Warehouse { get; set; } = "All Warehouses";

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
}