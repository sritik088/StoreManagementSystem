
using System;
using System.Collections.Generic;
using System.Linq;

namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class StockTransferReportViewModel
    {
        // =====================================================
        // SYSTEM / REPORT SETTINGS
        // =====================================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string ReportHeading { get; set; } = "Stock Transfer Report";

        public string FromDateLabel { get; set; } = "From Date";

        public string ToDateLabel { get; set; } = "To Date";

        public string AuthorisedSignatory { get; set; } = string.Empty;

        public string AuthorisedDesignation { get; set; } = string.Empty;


        // =====================================================
        // DATE FILTER
        // =====================================================

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }


        // =====================================================
        // TRANSFERS
        // =====================================================

        public List<StockTransferReportRowViewModel> Transfers { get; set; }
            = new List<StockTransferReportRowViewModel>();


        // =====================================================
        // SUMMARY
        // =====================================================

        public int TotalTransfers =>
            Transfers
                .Select(x => x.TransferNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Count();


        public decimal TotalOutwardQuantity =>
            Transfers
                .Where(x => x.TransferType == "Outward")
                .Sum(x => x.Quantity);


        public decimal TotalInwardQuantity =>
            Transfers
                .Where(x => x.TransferType == "Inward")
                .Sum(x => x.Quantity);


        public decimal TotalTransferValue =>
            Transfers.Sum(x => x.TotalValue);
    }


    public class StockTransferReportRowViewModel
    {
        public int ProductId { get; set; }

        public string TransferNo { get; set; } = string.Empty;

        public string TransferType { get; set; } = string.Empty;

        public DateTime TransferDate { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string MainCategoryName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string SubcategoryName { get; set; } = string.Empty;

        public int FromWarehouseId { get; set; }

        public string FromWarehouseName { get; set; } = string.Empty;

        public int ToWarehouseId { get; set; }

        public string ToWarehouseName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalValue =>
            Quantity * UnitCost;
    }
}