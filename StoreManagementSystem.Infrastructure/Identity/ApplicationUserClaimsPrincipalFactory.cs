using System.Security.Claims;


using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace StoreManagementSystem.Infrastructure.Identity;

public class ApplicationUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(
            userManager,
            roleManager,
            optionsAccessor)
    {
        _roleManager = roleManager;
    }


    protected override async Task<ClaimsIdentity>
        GenerateClaimsAsync(ApplicationUser user)
    {
        var identity =
            await base.GenerateClaimsAsync(user);


        // ---------------------------------------------------------
        // Get all roles assigned to the user
        // ---------------------------------------------------------

        var roles =
            await UserManager.GetRolesAsync(user);


        // ---------------------------------------------------------
        // Add permission claims from each role
        // ---------------------------------------------------------

        foreach (var roleName in roles)
        {
            var role =
                await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                continue;
            }

            var roleClaims =
                await _roleManager.GetClaimsAsync(role);

            foreach (var claim in roleClaims)
            {
                if (claim.Type ==
                    PermissionClaim.Type)
                {
                    identity.AddClaim(
                        new Claim(
                            claim.Type,
                            claim.Value));
                }
            }
        }

        return identity;
    }
}