
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace StoreManagementSystem.Infrastructure.Services
{
    public class SidebarMenuService : ISidebarMenuService
    {
        private readonly ISidebarMenuRepository _repository;

        public SidebarMenuService(
            ISidebarMenuRepository repository)
        {
            _repository = repository;
        }

        // =====================================================
        // GET MENU
        // =====================================================

        public Task<List<SidebarMenuSection>>
            GetSectionsAsync()
        {
            return _repository.GetSectionsAsync();
        }


        // =====================================================
        // SAVE ARRANGEMENT
        // =====================================================

        public Task SaveArrangementAsync(
            List<SidebarMenuSection> sections)
        {
            return _repository.SaveArrangementAsync(sections);
        }


        // =====================================================
        // SEED / UPDATE DEFAULT MENU
        // =====================================================

        public async Task SeedDefaultMenuAsync()
        {
            /*
             * IMPORTANT
             * -------------------------------------------------
             * Do NOT return when the database already contains
             * sidebar records.
             *
             * Existing users may already have their own
             * arrangement.
             *
             * Therefore:
             *
             * 1. Existing sections are NOT deleted.
             * 2. Existing positions are NOT overwritten.
             * 3. Missing sections are added.
             * 4. Missing menu items are added.
             *
             * This allows us to add new modules such as:
             *
             * ANALYTICS
             * ADMINISTRATION
             *
             * without destroying the current arrangement.
             */


            // =================================================
            // DASHBOARD
            // =================================================

            var dashboard = new SidebarMenuSection
            {
                Name = "DASHBOARD",
                DisplayOrder = 1,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Dashboard",
                        Controller = "Dashboard",
                        Action = "Index",
                        Icon = "bi-speedometer2",

                        // No permission = visible to all
                        Permission = null,

                        DisplayOrder = 1,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // MASTER DATA
            // =================================================

            var masterData = new SidebarMenuSection
            {
                Name = "MASTER DATA",
                DisplayOrder = 2,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Main Category",
                        Controller = "MainCategory",
                        Action = "Index",
                        Icon = "bi-diagram-3",
                        Permission = "MainCategoryView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Category",
                        Controller = "Category",
                        Action = "Index",
                        Icon = "bi-folder",
                        Permission = "CategoryView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Sub Category",
                        Controller = "SubCategory",
                        Action = "Index",
                        Icon = "bi-folder2-open",
                        Permission = "SubCategoryView",
                        DisplayOrder = 3,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Products",
                        Controller = "Product",
                        Action = "Index",
                        Icon = "bi-box-seam",
                        Permission = "ProductView",
                        DisplayOrder = 4,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Suppliers",
                        Controller = "Supplier",
                        Action = "Index",
                        Icon = "bi-truck",
                        Permission = "SupplierView",
                        DisplayOrder = 5,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Customers",
                        Controller = "Customer",
                        Action = "Index",
                        Icon = "bi-person-lines-fill",
                        Permission = "CustomerView",
                        DisplayOrder = 6,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Units",
                        Controller = "Unit",
                        Action = "Index",
                        Icon = "bi-rulers",
                        Permission = "UnitView",
                        DisplayOrder = 7,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Warehouses",
                        Controller = "Warehouse",
                        Action = "Index",
                        Icon = "bi-building",
                        Permission = "WarehouseView",
                        DisplayOrder = 8,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Damage Master",
                        Controller = "Damage",
                        Action = "Index",
                        Icon = "bi-exclamation-triangle",
                        Permission = "WarehouseView",
                        DisplayOrder = 9,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Damage Entry",
                        Controller = "DamageEntry",
                        Action = "Index",
                        Icon = "bi-box-seam",
                        Permission = "WarehouseView",
                        DisplayOrder = 10,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // PURCHASE
            // =================================================

            var purchase = new SidebarMenuSection
            {
                Name = "PURCHASE",
                DisplayOrder = 3,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Purchase Orders",
                        Controller = "PurchaseOrder",
                        Action = "Index",
                        Icon = "bi-cart-plus",
                        Permission = "PurchaseOrderView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Goods Receipt / GRN",
                        Controller = "GoodsReceipt",
                        Action = "Index",
                        Icon = "bi-box-arrow-in-down",
                        Permission = "GoodsReceiptView",
                        DisplayOrder = 2,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // INVENTORY
            // =================================================

            var inventory = new SidebarMenuSection
            {
                Name = "INVENTORY",
                DisplayOrder = 4,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Warehouse Stock",
                        Controller = "WarehouseStock",
                        Action = "Index",
                        Icon = "bi-boxes",
                        Permission = "WarehouseStockView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Stock Ledger",
                        Controller = "StockLedger",
                        Action = "Index",
                        Icon = "bi-journal-text",
                        Permission = "StockLedgerView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Stock Transfer",
                        Controller = "StockTransfer",
                        Action = "Index",
                        Icon = "bi-arrow-left-right",
                        Permission = "StockTransferView",
                        DisplayOrder = 3,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Product Statement",
                        Controller = "ProductStatement",
                        Action = "Index",
                        Icon = "bi-file-earmark-bar-graph",
                        Permission = "ProductStatementView",
                        DisplayOrder = 4,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // SALES
            // =================================================

            var sales = new SidebarMenuSection
            {
                Name = "SALES",
                DisplayOrder = 5,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Sales Orders",
                        Controller = "SalesOrder",
                        Action = "Index",
                        Icon = "bi-cart-check",
                        Permission = "SalesOrderView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Delivery Notes",
                        Controller = "DeliveryNote",
                        Action = "Index",
                        Icon = "bi-truck",
                        Permission = "DeliveryNoteView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Sales Invoices",
                        Controller = "SalesInvoice",
                        Action = "Index",
                        Icon = "bi-receipt-cutoff",
                        Permission = "SalesInvoiceView",
                        DisplayOrder = 3,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // ACCOUNTS PAYABLE
            // =================================================

            var accountsPayable = new SidebarMenuSection
            {
                Name = "ACCOUNTS PAYABLE",
                DisplayOrder = 6,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "AP Dashboard",
                        Controller = "AccountsPayable",
                        Action = "Index",
                        Icon = "bi-speedometer",
                        Permission = "APDashboardView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Supplier Outstanding",
                        Controller = "SupplierOutstanding",
                        Action = "Index",
                        Icon = "bi-exclamation-circle",
                        Permission = "SupplierOutstandingView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Supplier Ledger",
                        Controller = "SupplierLedger",
                        Action = "Index",
                        Icon = "bi-journal-text",
                        Permission = "SupplierLedgerView",
                        DisplayOrder = 3,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Supplier Payments",
                        Controller = "SupplierPayments",
                        Action = "Index",
                        Icon = "bi-cash-stack",
                        Permission = "SupplierPaymentsView",
                        DisplayOrder = 4,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // ACCOUNTS RECEIVABLE
            // =================================================

            var accountsReceivable = new SidebarMenuSection
            {
                Name = "ACCOUNTS RECEIVABLE",
                DisplayOrder = 7,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "AR Dashboard",
                        Controller = "AccountsReceivable",
                        Action = "Index",
                        Icon = "bi-speedometer",
                        Permission = "ARDashboardView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Customer Outstanding",
                        Controller = "CustomerOutstanding",
                        Action = "Index",
                        Icon = "bi-exclamation-circle",
                        Permission = "CustomerOutstandingView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Customer Receipts",
                        Controller = "CustomerReceipts",
                        Action = "Index",
                        Icon = "bi-cash-coin",
                        Permission = "CustomerReceiptsView",
                        DisplayOrder = 3,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Customer Ledger",
                        Controller = "CustomerLedger",
                        Action = "Index",
                        Icon = "bi-journal-text",
                        Permission = "CustomerLedgerView",
                        DisplayOrder = 4,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // REPORTS
            // =================================================

            var reports = new SidebarMenuSection
            {
                Name = "REPORTS",
                DisplayOrder = 8,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Reports Dashboard",
                        Controller = "Reports",
                        Action = "Dashboard",
                        Icon = "bi-bar-chart-line",
                        Permission = "ReportsDashboardView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Purchases Report",
                        Controller = "Reports",
                        Action = "Purchases",
                        Icon = "bi-cart",
                        Permission = "PurchasesReportView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Sales Report",
                        Controller = "Reports",
                        Action = "Sales",
                        Icon = "bi-graph-up",
                        Permission = "SalesReportView",
                        DisplayOrder = 3,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Stock Report",
                        Controller = "Reports",
                        Action = "Stock",
                        Icon = "bi-box-seam",
                        Permission = "StockReportView",
                        DisplayOrder = 4,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Inventory Valuation",
                        Controller = "InventoryValuation",
                        Action = "Index",
                        Icon = "bi-calculator",
                        Permission = "InventoryValuationView",
                        DisplayOrder = 5,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Low Stock",
                        Controller = "Reports",
                        Action = "LowStock",
                        Icon = "bi-exclamation-triangle",
                        Permission = "LowStockView",
                        DisplayOrder = 6,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Profit & Loss",
                        Controller = "Reports",
                        Action = "ProfitLoss",
                        Icon = "bi-currency-rupee",
                        Permission = "ProfitLossView",
                        DisplayOrder = 7,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "GST Report",
                        Controller = "Reports",
                        Action = "GST",
                        Icon = "bi-file-earmark-text",
                        Permission = "GSTReportView",
                        DisplayOrder = 8,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Customers Report",
                        Controller = "Reports",
                        Action = "Customers",
                        Icon = "bi-people",
                        Permission = "CustomersReportView",
                        DisplayOrder = 9,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Stock Transfer",
                        Controller = "Reports",
                        Action = "StockTransfer",
                        Icon = "bi-arrow-left-right",
                        Permission = "StockTransferView",
                        DisplayOrder = 10,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Suppliers Report",
                        Controller = "Reports",
                        Action = "Suppliers",
                        Icon = "bi-truck",
                        Permission = "SuppliersReportView",
                        DisplayOrder = 11,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // ANALYTICS
            // =================================================

            var analytics = new SidebarMenuSection
            {
                Name = "ANALYTICS",
                DisplayOrder = 9,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Analytics Dashboard",
                        Controller = "Analytics",
                        Action = "Index",
                        Icon = "bi-bar-chart-line-fill",
                        Permission = "AnalyticsDashboardView",
                        DisplayOrder = 1,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // ADMINISTRATION
            // =================================================

            var administration = new SidebarMenuSection
            {
                Name = "ADMINISTRATION",
                DisplayOrder = 10,
                IsActive = true,

                Items = new List<SidebarMenuItem>
                {
                    new SidebarMenuItem
                    {
                        Name = "Users",
                        Controller = "User",
                        Action = "Index",
                        Icon = "bi-people",
                        Permission = "UserView",
                        DisplayOrder = 1,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Roles & Permissions",
                        Controller = "Role",
                        Action = "Index",
                        Icon = "bi-shield-lock",
                        Permission = "RoleView",
                        DisplayOrder = 2,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Layouts Settings",
                        Controller = "Settings",
                        Action = "Index",
                        Icon = "bi-gear",
                        Permission = "SettingsView",
                        DisplayOrder = 3,
                        IsActive = true
                    },

                    new SidebarMenuItem
                    {
                        Name = "Sidebar Arrangement",
                        Controller = "Sidebar",
                        Action = "Index",
                        Icon = "bi-layout-sidebar",
                        Permission = "SidebarArrangementView",
                        DisplayOrder = 4,
                        IsActive = true
                    }
                }
            };


            // =================================================
            // DEFAULT MENU COLLECTION
            // =================================================

            var defaultSections =
                new List<SidebarMenuSection>
                {
                    dashboard,
                    masterData,
                    purchase,
                    inventory,
                    sales,
                    accountsPayable,
                    accountsReceivable,
                    reports,
                    analytics,
                    administration
                };


            // =================================================
            // GET EXISTING MENU
            // =================================================

            var existingSections =
                await _repository.GetSectionsAsync();


            // =================================================
            // DATABASE EMPTY
            // =================================================

            if (existingSections == null ||
                existingSections.Count == 0)
            {
                await _repository.AddSectionsAsync(
                    defaultSections);

                return;
            }


            // =================================================
            // ADD MISSING SECTIONS / ITEMS
            // =================================================

            var sectionsToAdd =
                new List<SidebarMenuSection>();


            foreach (var defaultSection in defaultSections)
            {
                var existingSection =
                    existingSections.FirstOrDefault(
                        x => x.Name.Equals(
                            defaultSection.Name,
                            StringComparison.OrdinalIgnoreCase));


                // -------------------------------------------------
                // SECTION DOES NOT EXIST
                // -------------------------------------------------

                if (existingSection == null)
                {
                    sectionsToAdd.Add(defaultSection);
                    continue;
                }


                // -------------------------------------------------
                // SECTION EXISTS
                // -------------------------------------------------

                var existingItems =
                    existingSection.Items?
                        .ToList()
                    ?? new List<SidebarMenuItem>();


                foreach (var defaultItem in defaultSection.Items)
                {
                    var itemExists =
                        existingItems.Any(
                            x =>
                                x.Controller.Equals(
                                    defaultItem.Controller,
                                    StringComparison.OrdinalIgnoreCase)
                                &&
                                x.Action.Equals(
                                    defaultItem.Action,
                                    StringComparison.OrdinalIgnoreCase));


                    // -------------------------------------------------
                    // ADD ONLY MISSING ITEM
                    // -------------------------------------------------

                    if (!itemExists)
                    {
                        defaultItem.SectionId =
                            existingSection.Id;

                        defaultItem.Section = null;

                        existingSection.Items.Add(
                            defaultItem);
                    }
                }
            }


            // =================================================
            // ADD MISSING SECTIONS
            // =================================================

            if (sectionsToAdd.Count > 0)
            {
                await _repository.AddSectionsAsync(
                    sectionsToAdd);
            }
        }
    }
}
