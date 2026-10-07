namespace StoreManagementSystem.Infrastructure.Identity;

public static class PermissionConstants
{
    public const string DashboardView = "Dashboard.View";

    public const string MainCategoryView = "MainCategory.View";
    public const string CategoryView = "Category.View";
    public const string SubCategoryView = "SubCategory.View";
    public const string ProductView = "Product.View";
    public const string SupplierView = "Supplier.View";
    public const string CustomerView = "Customer.View";
    public const string UnitView = "Unit.View";
    public const string WarehouseView = "Warehouse.View";

    public const string PurchaseOrderView = "PurchaseOrder.View";
    public const string GoodsReceiptView = "GoodsReceipt.View";
    public const string PurchaseInvoiceView = "PurchaseInvoice.View";

    public const string WarehouseStockView = "WarehouseStock.View";
    public const string StockLedgerView = "StockLedger.View";
    public const string StockTransferView = "StockTransfer.View";
    public const string StockAdjustmentView = "StockAdjustment.View";
    public const string ProductStatementView = "ProductStatement.View";

    public const string SalesOrderView = "SalesOrder.View";
    public const string DeliveryNoteView = "DeliveryNote.View";
    public const string SalesInvoiceView = "SalesInvoice.View";

    public const string APDashboardView = "APDashboard.View";
    public const string SupplierOutstandingView = "SupplierOutstanding.View";
    public const string SupplierLedgerView = "SupplierLedger.View";
    public const string SupplierPaymentsView = "SupplierPayments.View";

    public const string ARDashboardView = "ARDashboard.View";
    public const string CustomerOutstandingView = "CustomerOutstanding.View";
    public const string CustomerReceiptsView = "CustomerReceipts.View";
    public const string CustomerLedgerView = "CustomerLedger.View";

    public const string ReportsDashboardView = "ReportsDashboard.View";
    public const string PurchasesReportView = "PurchasesReport.View";
    public const string SalesReportView = "SalesReport.View";
    public const string StockReportView = "StockReport.View";
    public const string InventoryValuationView = "InventoryValuation.View";
    public const string LowStockView = "LowStock.View";
    public const string ProfitLossView = "ProfitLoss.View";
    public const string GSTReportView = "GSTReport.View";
    public const string CustomersReportView = "CustomersReport.View";
    public const string SuppliersReportView = "SuppliersReport.View";

    public const string UserManagementView = "UserManagement.View";
    public const string RolesPermissionsView = "RolesPermissions.View";
    public const string SettingsView = "Settings.View";

    public static readonly Dictionary<string, string> All =
        new()
        {
            { DashboardView, "Dashboard" },

            { MainCategoryView, "Main Category" },
            { CategoryView, "Category" },
            { SubCategoryView, "Sub Category" },
            { ProductView, "Products" },
            { SupplierView, "Suppliers" },
            { CustomerView, "Customers" },
            { UnitView, "Units" },
            { WarehouseView, "Warehouses" },

            { PurchaseOrderView, "Purchase Orders" },
            { GoodsReceiptView, "Goods Receipt / GRN" },
            { PurchaseInvoiceView, "Purchase Invoices" },

            { WarehouseStockView, "Warehouse Stock" },
            { StockLedgerView, "Stock Ledger" },
            { StockTransferView, "Stock Transfer" },
            { StockAdjustmentView, "Stock Adjustment" },
            { ProductStatementView, "Product Statement" },

            { SalesOrderView, "Sales Orders" },
            { DeliveryNoteView, "Delivery Notes" },
            { SalesInvoiceView, "Sales Invoices" },

            { APDashboardView, "AP Dashboard" },
            { SupplierOutstandingView, "Supplier Outstanding" },
            { SupplierLedgerView, "Supplier Ledger" },
            { SupplierPaymentsView, "Supplier Payments" },

            { ARDashboardView, "AR Dashboard" },
            { CustomerOutstandingView, "Customer Outstanding" },
            { CustomerReceiptsView, "Customer Receipts" },
            { CustomerLedgerView, "Customer Ledger" },

            { ReportsDashboardView, "Reports Dashboard" },
            { PurchasesReportView, "Purchases Report" },
            { SalesReportView, "Sales Report" },
            { StockReportView, "Stock Report" },
            { InventoryValuationView, "Inventory Valuation" },
            { LowStockView, "Low Stock" },
            { ProfitLossView, "Profit & Loss" },
            { GSTReportView, "GST Report" },
            { CustomersReportView, "Customers Report" },
            { SuppliersReportView, "Suppliers Report" },

            { UserManagementView, "User Management" },
            { RolesPermissionsView, "Roles & Permissions" },
            { SettingsView, "Settings" }
        };
}