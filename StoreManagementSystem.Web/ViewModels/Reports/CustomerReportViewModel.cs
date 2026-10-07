namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class CustomerReportViewModel
    {
        public int TotalCustomers { get; set; }

        public decimal TotalSales { get; set; }

        public decimal TotalReceived { get; set; }

        public decimal TotalOutstanding { get; set; }
    }
}