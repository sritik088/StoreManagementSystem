using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Web.ViewModels.Role;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [Route("Role")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }


        // =========================================================
        // ROLE LIST
        // =========================================================

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var roles = _roleManager.Roles
                .OrderBy(x => x.Name)
                .ToList();

            var model = new List<RoleListViewModel>();

            foreach (var role in roles)
            {
                var users =
                    await _userManager.GetUsersInRoleAsync(
                        role.Name!);

                var permissionCount =
                    await GetPermissionCountAsync(role);

                model.Add(new RoleListViewModel
                {
                    Id = role.Id,

                    Name =
                        role.Name ?? string.Empty,

                    UserCount =
                        users.Count,

                    PermissionCount =
                        permissionCount
                });
            }

            return View(model);
        }


        // =========================================================
        // CREATE ROLE - GET
        // =========================================================

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CREATE ROLE - POST
        // =========================================================

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "name",
                    "Role name is required.");

                return View();
            }

            name = name.Trim();

            if (await _roleManager.RoleExistsAsync(name))
            {
                ModelState.AddModelError(
                    "name",
                    "This role already exists.");

                return View();
            }

            var result =
                await _roleManager.CreateAsync(
                    new IdentityRole(name));

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Role created successfully.";

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View();
        }


        // =========================================================
        // DELETE ROLE
        // =========================================================

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role =
                await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                TempData["Error"] =
                    "Role not found.";

                return RedirectToAction(nameof(Index));
            }

            if (role.Name ==
                RoleConstants.SuperAdmin)
            {
                TempData["Error"] =
                    "SuperAdmin role cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }

            var users =
                await _userManager.GetUsersInRoleAsync(
                    role.Name!);

            if (users.Count > 0)
            {
                TempData["Error"] =
                    "This role cannot be deleted because users are assigned to it.";

                return RedirectToAction(nameof(Index));
            }

            var result =
                await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Role deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Unable to delete role.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // PERMISSIONS - GET
        // =========================================================

        [HttpGet("Permissions/{id}")]
        public async Task<IActionResult> Permissions(string id)
        {
            var role =
                await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                TempData["Error"] =
                    "Role not found.";

                return RedirectToAction(nameof(Index));
            }

            var permissions =
                await BuildPermissionListAsync(role);

            var model =
                new RolePermissionsViewModel
                {
                    RoleId =
                        role.Id,

                    RoleName =
                        role.Name ?? string.Empty,

                    Permissions =
                        permissions
                };

            return View(model);
        }


        // =========================================================
        // PERMISSIONS - POST / SAVE
        // =========================================================

        [HttpPost("Permissions/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(
            string id,
            RolePermissionsViewModel model)
        {
            // -----------------------------------------------------
            // FIND ROLE
            // -----------------------------------------------------

            var role =
                await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                TempData["Error"] =
                    "Role not found.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // SUPERADMIN PROTECTION
            // -----------------------------------------------------

            if (role.Name ==
                RoleConstants.SuperAdmin)
            {
                TempData["Error"] =
                    "SuperAdmin permissions cannot be changed.";

                return RedirectToAction(
                    nameof(Permissions),
                    new
                    {
                        id = role.Id
                    });
            }


            // -----------------------------------------------------
            // CHECK MODEL
            // -----------------------------------------------------

            if (model.Permissions == null)
            {
                TempData["Error"] =
                    "No permissions were submitted.";

                return RedirectToAction(
                    nameof(Permissions),
                    new
                    {
                        id = role.Id
                    });
            }


            // -----------------------------------------------------
            // GET CURRENT CLAIMS
            // -----------------------------------------------------

            var existingClaims =
                await _roleManager.GetClaimsAsync(role);

            var oldPermissionClaims =
                existingClaims
                    .Where(x =>
                        x.Type ==
                        PermissionClaim.Type)
                    .ToList();


            // -----------------------------------------------------
            // REMOVE OLD PERMISSIONS
            // -----------------------------------------------------

            foreach (var claim in oldPermissionClaims)
            {
                var removeResult =
                    await _roleManager.RemoveClaimAsync(
                        role,
                        claim);

                if (!removeResult.Succeeded)
                {
                    TempData["Error"] =
                        "Unable to remove existing permissions.";

                    return RedirectToAction(
                        nameof(Permissions),
                        new
                        {
                            id = role.Id
                        });
                }
            }


            // -----------------------------------------------------
            // GET SELECTED PERMISSIONS
            // -----------------------------------------------------

            var selectedPermissions =
                model.Permissions
                    .Where(x => x.IsSelected)
                    .Select(x => x.Key)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();


            // -----------------------------------------------------
            // ADD SELECTED PERMISSIONS
            // -----------------------------------------------------

            foreach (var permission
                     in selectedPermissions)
            {
                // Only permissions defined in
                // PermissionConstants.All are accepted.

                if (!PermissionConstants.All.ContainsKey(
                        permission))
                {
                    continue;
                }

                var addResult =
                    await _roleManager.AddClaimAsync(
                        role,
                        new Claim(
                            PermissionClaim.Type,
                            permission));

                if (!addResult.Succeeded)
                {
                    TempData["Error"] =
                        "Unable to save permission: " +
                        permission;

                    return RedirectToAction(
                        nameof(Permissions),
                        new
                        {
                            id = role.Id
                        });
                }
            }


            // -----------------------------------------------------
            // REFRESH USER SECURITY STAMP
            // -----------------------------------------------------
            // Users assigned to this role will receive
            // updated permission claims after signing in again.

            var users =
                await _userManager.GetUsersInRoleAsync(
                    role.Name!);

            foreach (var user in users)
            {
                await _userManager.UpdateSecurityStampAsync(
                    user);
            }


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            TempData["Success"] =
                "Role permissions saved successfully.";

            return RedirectToAction(
                nameof(Permissions),
                new
                {
                    id = role.Id
                });
        }


        // =========================================================
        // BUILD PERMISSION LIST
        // =========================================================

        private async Task<List<PermissionItemViewModel>>
            BuildPermissionListAsync(
                IdentityRole role)
        {
            var result =
                new List<PermissionItemViewModel>();

            var existingClaims =
                await _roleManager.GetClaimsAsync(role);

            var selected =
                existingClaims
                    .Where(x =>
                        x.Type ==
                        PermissionClaim.Type)
                    .Select(x => x.Value)
                    .ToHashSet();


            foreach (var permission
                     in PermissionConstants.All)
            {
                var parts =
                    permission.Key.Split('.');

                var module =
                    parts.Length > 0
                        ? parts[0]
                        : "Other";


                result.Add(
                    new PermissionItemViewModel
                    {
                        Key =
                            permission.Key,

                        Name =
                            permission.Value,

                        Module =
                            module,

                        IsSelected =
                            role.Name ==
                            RoleConstants.SuperAdmin
                            ||
                            selected.Contains(
                                permission.Key)
                    });
            }

            return result;
        }


        // =========================================================
        // PERMISSION COUNT
        // =========================================================

        private async Task<int>
            GetPermissionCountAsync(
                IdentityRole role)
        {
            if (role.Name ==
                RoleConstants.SuperAdmin)
            {
                return PermissionConstants.All.Count;
            }

            var claims =
                await _roleManager.GetClaimsAsync(
                    role);

            return claims.Count(
                x =>
                    x.Type ==
                    PermissionClaim.Type);
        }
    }
}