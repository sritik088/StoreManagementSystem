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
                await roleManager.CreateAsync(new IdentityRole(role));
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
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, password);

            await userManager.AddToRoleAsync(admin,
                RoleConstants.SuperAdmin);
        }
    }
}
