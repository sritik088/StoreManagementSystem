namespace StoreManagementSystem.Web.ViewModels.Role;

public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public List<PermissionItemViewModel> Permissions { get; set; }
        = new();
}


public class PermissionItemViewModel
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public bool IsSelected { get; set; }
}