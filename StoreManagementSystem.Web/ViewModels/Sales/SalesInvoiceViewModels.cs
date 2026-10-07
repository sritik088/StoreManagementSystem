using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.Sales
{
    public class SalesInvoiceIndexViewModel
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public string SalesOrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public decimal GrandTotal { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal BalanceDue { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }


    public class SalesInvoiceCreateViewModel
    {
        [Required]
        [Display(Name = "Sales Order")]
        public int SalesOrderId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(30)]
        public string PaymentMode { get; set; } = "Cash";

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public List<SelectListItem> SalesOrders { get; set; }
            = new List<SelectListItem>();
    }


    public class SalesInvoiceDetailsViewModel
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public string SalesOrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        public decimal PaidAmount { get; set; }

        public decimal BalanceDue { get; set; }

        public string PaymentReference { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;

        public List<SalesInvoiceItemViewModel> Items { get; set; }
            = new List<SalesInvoiceItemViewModel>();
    }


    public class SalesInvoiceItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxPercent { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal LineTotal { get; set; }
    }
}