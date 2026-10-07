namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class DashboardReportViewModel
    {
        public int TotalProducts { get; set; }

        public int TotalSuppliers { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalWarehouses { get; set; }

        public decimal TotalPurchase { get; set; }

        public decimal TotalSales { get; set; }

        public decimal TotalProfit { get; set; }

        public decimal OutstandingAmount { get; set; }
    }
}