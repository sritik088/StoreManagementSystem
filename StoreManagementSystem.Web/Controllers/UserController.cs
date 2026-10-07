using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Infrastructure.Identity;

using StoreManagementSystem.Web.ViewModels.User;

namespace StoreManagementSystem.Web.Controllers;

[Authorize(Roles = RoleConstants.SuperAdmin)]
[Route("User")]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public UserController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .Include(x => x.Warehouse)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync();

        var result = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserListViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "No Role",
                Warehouse = user.Warehouse?.Name ?? "All Warehouses",
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            });
        }

        return View(result);
    }

    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel();

        await LoadCreateDropdowns(model);

        return View(model);
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCreateDropdowns(model);
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                "Email",
                "A user with this email already exists.");

            await LoadCreateDropdowns(model);
            return View(model);
        }

        // Validate role
        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(
                "Role",
                "Selected role does not exist.");

            await LoadCreateDropdowns(model);
            return View(model);
        }

        // Validate warehouse if selected
        if (model.WarehouseId.HasValue)
        {
            var warehouseExists = await _context.Warehouses
                .AnyAsync(x => x.Id == model.WarehouseId.Value);

            if (!warehouseExists)
            {
                ModelState.AddModelError(
                    "WarehouseId",
                    "Selected warehouse does not exist.");

                await LoadCreateDropdowns(model);
                return View(model);
            }
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address,
            WarehouseId = model.WarehouseId,
            IsActive = model.IsActive,
            CreatedDate = DateTime.Now,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(
            user,
            model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            await LoadCreateDropdowns(model);
            return View(model);
        }

        await _userManager.AddToRoleAsync(
            user,
            model.Role);

        TempData["Success"] =
            "User created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            WarehouseId = user.WarehouseId,
            IsActive = user.IsActive,
            Role = roles.FirstOrDefault() ?? string.Empty
        };

        await LoadEditDropdowns(model);

        return View(model);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadEditDropdowns(model);
            return View(model);
        }

        // Check email duplication
        var emailUser = await _userManager.FindByEmailAsync(model.Email);

        if (emailUser != null && emailUser.Id != user.Id)
        {
            ModelState.AddModelError(
                "Email",
                "Another user already uses this email.");

            await LoadEditDropdowns(model);
            return View(model);
        }

        // Validate role
        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(
                "Role",
                "Selected role does not exist.");

            await LoadEditDropdowns(model);
            return View(model);
        }

        // Validate warehouse
        if (model.WarehouseId.HasValue)
        {
            var warehouseExists = await _context.Warehouses
                .AnyAsync(x => x.Id == model.WarehouseId.Value);

            if (!warehouseExists)
            {
                ModelState.AddModelError(
                    "WarehouseId",
                    "Selected warehouse does not exist.");

                await LoadEditDropdowns(model);
                return View(model);
            }
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.WarehouseId = model.WarehouseId;
        user.IsActive = model.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            await LoadEditDropdowns(model);
            return View(model);
        }

        // Update role
        var currentRoles =
            await _userManager.GetRolesAsync(user);

        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(
                user,
                currentRoles);
        }

        await _userManager.AddToRoleAsync(
            user,
            model.Role);

        TempData["Success"] =
            "User updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // DEACTIVATE
    // =========================================================

    [HttpPost("Deactivate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Do not allow SuperAdmin to deactivate itself
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] =
                "You cannot deactivate your own account.";

            return RedirectToAction(nameof(Index));
        }

        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] =
                "User deactivated successfully.";
        }
        else
        {
            TempData["Error"] =
                "Unable to deactivate user.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // ACTIVATE
    // =========================================================

    [HttpPost("Activate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = true;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] =
                "User activated successfully.";
        }
        else
        {
            TempData["Error"] =
                "Unable to activate user.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // DELETE
    // =========================================================

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] =
                "You cannot delete your own account.";

            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] =
                "User deleted successfully.";
        }
        else
        {
            TempData["Error"] =
                "Unable to delete user.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // DROPDOWNS - CREATE
    // =========================================================

    private async Task LoadCreateDropdowns(
        CreateUserViewModel model)
    {
        model.Roles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Name!,
                Text = x.Name!
            })
            .ToListAsync();

        model.Warehouses = await _context.Warehouses
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();
    }

    // =========================================================
    // DROPDOWNS - EDIT
    // =========================================================

    private async Task LoadEditDropdowns(
        EditUserViewModel model)
    {
        model.Roles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Name!,
                Text = x.Name!,
                Selected = x.Name == model.Role
            })
            .ToListAsync();

        model.Warehouses = await _context.Warehouses
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
                Selected = x.Id == model.WarehouseId
            })
            .ToListAsync();
    }
}