using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Application.Services;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Infrastructure.Repositories;
using StoreManagementSystem.Infrastructure.Services;
using StoreManagementSystem.Infrastructure.UnitOfWork;
using StoreManagementSystem.Web.Authorization;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
builder.Services.AddScoped<IWarehouseStockRepository, WarehouseStockRepository>();
builder.Services.AddScoped<IWarehouseStockService, WarehouseStockService>();
builder.Services.AddScoped<IStockLedgerRepository, StockLedgerRepository>();
builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<
    ICustomerRepository,
    CustomerRepository>();

builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

builder.Services.AddScoped<
    ISalesOrderRepository,
    SalesOrderRepository>();

builder.Services.AddScoped<
    ISalesOrderService,
    SalesOrderService>();

builder.Services.AddScoped<IMainCategoryRepository,
    MainCategoryRepository>();

builder.Services.AddScoped<IMainCategoryService,
    MainCategoryService>();

builder.Services.AddScoped<IStockLedgerService, StockLedgerService>();

builder.Services.AddScoped<IDeliveryRepository,
    DeliveryRepository>();

builder.Services.AddScoped<IDeliveryService,
    DeliveryService>();

builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

// =========================================================
// SIDEBAR
// =========================================================

builder.Services.AddScoped<
    ISidebarMenuRepository,
    SidebarMenuRepository>();

builder.Services.AddScoped<
    ISidebarMenuService,
    SidebarMenuService>();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// ============================================================
// DATABASE INITIALIZATION
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedAsync(
        userManager,
        roleManager);

    var sidebarMenuService =
        services.GetRequiredService<ISidebarMenuService>();

    await sidebarMenuService.SeedDefaultMenuAsync();
}




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedAsync(userManager, roleManager);
}

app.Run();