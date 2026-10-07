using Microsoft.AspNetCore.Authorization;

namespace StoreManagementSystem.Web.Authorization;

public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionAuthorizeAttribute(string permission)
    {
        Policy = permission;
    }
}