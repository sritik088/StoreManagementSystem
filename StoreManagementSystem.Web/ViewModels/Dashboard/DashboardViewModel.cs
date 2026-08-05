namespace StoreManagementSystem.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        // Statistics
        public int TotalProducts { get; set; }

        public int TotalCategories { get; set; }

        public int TotalSubCategories { get; set; }

        public int TotalSuppliers { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalPurchaseOrders { get; set; }

        public int TotalSalesOrders { get; set; }

        public int LowStockProducts { get; set; }

        public int OutOfStockProducts { get; set; }

        // Revenue
        public decimal TodaySales { get; set; }

        public decimal MonthlySales { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalPurchaseAmount { get; set; }

        // Inventory
        public int TotalStockQuantity { get; set; }

        public decimal InventoryValue { get; set; }

        // Dashboard Lists (for future use)
        public List<RecentPurchaseViewModel> RecentPurchases { get; set; } = new();

        public List<LowStockViewModel> LowStockItems { get; set; } = new();
    }

    public class RecentPurchaseViewModel
    {
        public int Id { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class LowStockViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public int Stock { get; set; }

        public int MinimumStock { get; set; }
    }
}
