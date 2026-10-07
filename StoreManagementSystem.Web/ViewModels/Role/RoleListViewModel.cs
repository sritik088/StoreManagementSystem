namespace StoreManagementSystem.Web.ViewModels.Role;

public class RoleListViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int UserCount { get; set; }

    public int PermissionCount { get; set; }
}