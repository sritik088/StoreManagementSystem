
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class PurchaseReportViewModel
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? SupplierId { get; set; }

        public string? Status { get; set; }

        public int TotalOrders { get; set; }

        public decimal TotalPurchase { get; set; }

        public decimal TotalTax { get; set; }

        public decimal TotalDiscount { get; set; }

        public List<PurchaseReportRowViewModel> Orders { get; set; }
            = new List<PurchaseReportRowViewModel>();

        public List<SupplierReportFilterViewModel> Suppliers { get; set; }
            = new List<SupplierReportFilterViewModel>();
    }


    public class PurchaseReportRowViewModel
    {
        public int Id { get; set; }

        public string PONumber { get; set; }
            = string.Empty;

        public DateTime OrderDate { get; set; }

        public DateTime ExpectedDate { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string Status { get; set; }
            = string.Empty;
    }


    public class SupplierReportFilterViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;
    }
}

