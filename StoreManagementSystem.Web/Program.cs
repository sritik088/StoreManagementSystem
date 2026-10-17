
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Application.Services;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Infrastructure.Repositories;
using StoreManagementSystem.Infrastructure.Services;
using StoreManagementSystem.Infrastructure.Settings;
using StoreManagementSystem.Infrastructure.UnitOfWork;
using StoreManagementSystem.Web.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// KESTREL SECURITY
// ============================================================

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();

// ============================================================
// DATABASE
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================================
// ASP.NET CORE IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(10);

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
// AUTHENTICATION COOKIE
// ============================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    // Host-prefixed cookie requires HTTPS, Path=/ and no Domain.
    options.Cookie.Name = "__Host-StoreManagement.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = "/";

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";

    // Authentication lifetime
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;

    // Preserve Identity's normal security-stamp validation.
    // Do not replace the cookie events with a custom event handler
    // unless your application needs additional cookie events.
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(5);
});

// ============================================================
// AUTHORIZATION
// Require authentication by default.
// Explicit [AllowAnonymous] endpoints remain public.
// ============================================================

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    ApplicationUserClaimsPrincipalFactory>();

// ============================================================
// CATEGORY
// ============================================================

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();

builder.Services.AddScoped<IMainCategoryRepository, MainCategoryRepository>();
builder.Services.AddScoped<IMainCategoryService, MainCategoryService>();

// ============================================================
// SUPPLIER
// ============================================================

builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();

// ============================================================
// UNIT
// ============================================================

builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUnitService, UnitService>();

// ============================================================
// WAREHOUSE
// ============================================================

builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

// ============================================================
// PRODUCT
// ============================================================

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// ============================================================
// GOODS RECEIPT
// ============================================================

builder.Services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();

// ============================================================
// PURCHASE ORDER
// ============================================================

builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

// ============================================================
// WAREHOUSE STOCK
// ============================================================

builder.Services.AddScoped<
    IWarehouseStockRepository,
    WarehouseStockRepository>();

builder.Services.AddScoped<
    IWarehouseStockService,
    WarehouseStockService>();

// ============================================================
// STOCK LEDGER
// ============================================================

builder.Services.AddScoped<IStockLedgerRepository, StockLedgerRepository>();
builder.Services.AddScoped<IStockLedgerService, StockLedgerService>();

// ============================================================
// STOCK TRANSFER
// ============================================================

builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();

// ============================================================
// CUSTOMER
// ============================================================

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// ============================================================
// SALES ORDER
// ============================================================

builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();

// ============================================================
// DELIVERY
// ============================================================

builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

// ============================================================
// UNIT OF WORK
// ============================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============================================================
// SYSTEM SETTINGS
// ============================================================

builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

// ============================================================
// SIDEBAR MENU
// ============================================================

builder.Services.AddScoped<ISidebarMenuRepository, SidebarMenuRepository>();
builder.Services.AddScoped<ISidebarMenuService, SidebarMenuService>();

// ============================================================
// EMAIL
// ============================================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailService, SmtpEmailService>();

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// ERROR HANDLING AND HTTPS
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ============================================================
// SECURITY HEADERS AND CACHE CONTROL
// Keep this middleware before static files and routing.
// No CSP is assigned or overridden here.
// ============================================================

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] =
            "strict-origin-when-cross-origin";

        headers["Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=()";

        // Do not set Content-Security-Policy here.
        // Preserve your existing CSP configuration.

        // Disable caching for dynamic application and account pages.
        // Static files are handled below and can retain their own policy.
        var path = context.Request.Path;

        var isAccountRequest =
            path.StartsWithSegments("/Account");

        var isStaticFile =
            Path.HasExtension(path.Value ?? string.Empty);

        if (isAccountRequest || !isStaticFile)
        {
            headers["Cache-Control"] =
                "no-store, no-cache, max-age=0, must-revalidate, private";

            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }

        headers.Remove("X-Powered-By");
        headers.Remove("Server");

        return Task.CompletedTask;
    });

    await next();
});

// ============================================================
// STATIC FILES
// ============================================================

app.UseStaticFiles();

// ============================================================
// ROUTING
// ============================================================

app.UseRouting();

// ============================================================
// AUTHENTICATION AND AUTHORIZATION
// ============================================================

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// DATABASE / IDENTITY / SIDEBAR INITIALIZATION
// Seed only once.
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

// ============================================================
// DEFAULT ROUTE
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
