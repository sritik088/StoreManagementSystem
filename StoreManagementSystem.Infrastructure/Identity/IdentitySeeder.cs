using Microsoft.AspNetCore.Identity;

namespace StoreManagementSystem.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            RoleConstants.SuperAdmin,
            RoleConstants.StoreManager,
            RoleConstants.WarehouseManager,
            RoleConstants.PurchaseManager,
            RoleConstants.InventoryManager,
            RoleConstants.SalesManager,
            RoleConstants.Cashier,
            RoleConstants.Accountant
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }

        const string email = "admin@store.com";
        const string password = "Admin@123";

        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,

                FirstName = "System",
                LastName = "Administrator",

                EmailConfirmed = true,

                IsActive = true,

                CreatedDate = DateTime.Now,

                // SuperAdmin has access to all warehouses
                WarehouseId = null
            };

            var result = await userManager.CreateAsync(
                admin,
                password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    admin,
                    RoleConstants.SuperAdmin);
            }
        }
        else
        {
            // Make sure existing admin remains active
            if (!admin.IsActive)
            {
                admin.IsActive = true;

                await userManager.UpdateAsync(admin);
            }

            var rolesForAdmin =
                await userManager.GetRolesAsync(admin);

            if (!rolesForAdmin.Contains(
                    RoleConstants.SuperAdmin))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    RoleConstants.SuperAdmin);
            }
        }
    }
}