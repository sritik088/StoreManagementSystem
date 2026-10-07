using System;

namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class SalesReportViewModel
    {
        // =========================================================
        // SYSTEM / REPORT SETTINGS
        // =========================================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string ReportHeading { get; set; } = "Sales Report";

        public string FromDateLabel { get; set; } = "From Date";

        public string ToDateLabel { get; set; } = "To Date";

        public string AuthorisedSignatory { get; set; } = string.Empty;

        public string AuthorisedDesignation { get; set; } = string.Empty;


        // =========================================================
        // REPORT DATE RANGE
        // =========================================================

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }


        // =========================================================
        // SALES VALUES
        // =========================================================

        /// <summary>
        /// Gross sales before discount and tax.
        /// Quantity × SalePrice
        /// </summary>
        public decimal TotalSales { get; set; }


        /// <summary>
        /// Total discount given on sales.
        /// </summary>
        public decimal TotalDiscount { get; set; }


        /// <summary>
        /// Net sales after discount but before tax.
        /// TotalSales - TotalDiscount
        /// </summary>
        public decimal NetSales { get; set; }


        /// <summary>
        /// Total tax charged on net sales.
        /// Tax does not form part of Gross Profit.
        /// </summary>
        public decimal TotalTax { get; set; }


        /// <summary>
        /// Final invoice value.
        /// NetSales + TotalTax
        /// </summary>
        public decimal GrandTotal { get; set; }


        // =========================================================
        // COST OF GOODS SOLD
        // =========================================================

        /// <summary>
        /// Actual purchase/GRN cost of confirmed sales.
        /// Quantity × SalesOrderItem.UnitPrice
        /// </summary>
        public decimal CostOfGoodsSold { get; set; }


        // =========================================================
        // PROFITABILITY
        // =========================================================

        /// <summary>
        /// Gross Profit.
        /// NetSales - CostOfGoodsSold
        /// </summary>
        public decimal GrossProfit { get; set; }


        /// <summary>
        /// Gross Profit Margin percentage.
        /// GrossProfit / NetSales × 100
        /// </summary>
        public decimal GrossMarginPercent { get; set; }


        // =========================================================
        // INVOICE COUNT
        // =========================================================

        public int TotalInvoices { get; set; }
    }
}