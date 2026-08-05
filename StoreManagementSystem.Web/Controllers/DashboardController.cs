using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Web.ViewModels.Dashboard;

namespace StoreManagementSystem.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalProducts = 150,
                TotalCategories = 12,
                TotalSubCategories = 35,
                TotalSuppliers = 20,
                TotalCustomers = 85,
                TotalPurchaseOrders = 40,
                TotalSalesOrders = 120,
                LowStockProducts = 8,
                OutOfStockProducts = 2,

                TodaySales = 12500,
                MonthlySales = 245000,
                TotalRevenue = 1520000,
                TotalPurchaseAmount = 980000,

                TotalStockQuantity = 4500,
                InventoryValue = 750000
            };

            return View(model);
        }
    }
}
