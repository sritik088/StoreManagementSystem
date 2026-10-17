
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.ProductStatement;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ProductStatementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductStatementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // PRODUCT STATEMENT
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var model = new ProductStatementViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            // ========================================================
            // STEP 1: VALIDATE DATE RANGE
            // ========================================================

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                ModelState.AddModelError(
                    "",
                    "From Date cannot be greater than To Date.");

                return View(model);
            }

            // ========================================================
            // STEP 2: GET PURCHASE TRANSACTIONS
            // ========================================================

            var allPurchases = await _context.GoodsReceiptItems
                .AsNoTracking()
                .Where(x => x.GoodsReceipt != null)
                .Select(x => new
                {
                    Date = x.GoodsReceipt!.ReceiptDate,

                    GRNNumber = x.GoodsReceipt.GRNNumber,

                    ProductTitle = x.Product != null
                        ? x.Product.Title
                        : "-",

                    Quantity = x.ReceivedQuantity,

                    Rate = x.UnitPrice
                })
                .ToListAsync();

            // ========================================================
            // STEP 3: GET SALES TRANSACTIONS
            // ========================================================

            var allSales = await _context.SalesOrderItems
                .AsNoTracking()
                .Where(x => x.SalesOrder != null)
                .Select(x => new
                {
                    Date = x.SalesOrder!.OrderDate,

                    ProductTitle = x.Product != null
                        ? x.Product.Title
                        : "-",

                    Quantity = x.Quantity,

                    Rate = x.UnitPrice
                })
                .ToListAsync();

            // ========================================================
            // STEP 4: GET DAMAGE TRANSACTIONS
            // ========================================================

            var allDamages = await _context.DamageEntries
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    Date = x.DamageDate,

                    DamageNumber = x.DamageNumber,

                    ProductTitle = x.Product != null
                        ? x.Product.Title
                        : "-",

                    Quantity = x.Quantity,

                    Rate = x.UnitCost
                })
                .ToListAsync();

            // ========================================================
            // STEP 5: CALCULATE OPENING BALANCE
            // ========================================================

            decimal openingQuantity = 0m;
            decimal openingRate = 0m;

            if (fromDate.HasValue)
            {
                DateTime openingDate = fromDate.Value.Date;

                var openingTransactions =
                    new List<OpeningTransaction>();

                foreach (var purchase in allPurchases
                    .Where(x => x.Date.Date < openingDate))
                {
                    openingTransactions.Add(
                        new OpeningTransaction
                        {
                            Date = purchase.Date,
                            TransactionType = TransactionType.Purchase,
                            Quantity = purchase.Quantity,
                            Rate = purchase.Rate
                        });
                }

                foreach (var sale in allSales
                    .Where(x => x.Date.Date < openingDate))
                {
                    openingTransactions.Add(
                        new OpeningTransaction
                        {
                            Date = sale.Date,
                            TransactionType = TransactionType.Sale,
                            Quantity = sale.Quantity,
                            Rate = sale.Rate
                        });
                }

                foreach (var damage in allDamages
                    .Where(x => x.Date.Date < openingDate))
                {
                    openingTransactions.Add(
                        new OpeningTransaction
                        {
                            Date = damage.Date,
                            TransactionType = TransactionType.Damage,
                            Quantity = damage.Quantity,
                            Rate = damage.Rate
                        });
                }

                openingTransactions = openingTransactions
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.TransactionType)
                    .ToList();

                foreach (var transaction in openingTransactions)
                {
                    if (transaction.TransactionType ==
                        TransactionType.Purchase)
                    {
                        decimal previousValue =
                            openingQuantity * openingRate;

                        decimal purchaseValue =
                            transaction.Quantity * transaction.Rate;

                        openingQuantity += transaction.Quantity;

                        if (openingQuantity > 0)
                        {
                            openingRate =
                                (previousValue + purchaseValue)
                                / openingQuantity;
                        }
                    }
                    else if (
                        transaction.TransactionType ==
                        TransactionType.Sale ||
                        transaction.TransactionType ==
                        TransactionType.Damage)
                    {
                        openingQuantity -= transaction.Quantity;

                        if (openingQuantity <= 0)
                        {
                            openingQuantity = 0m;
                            openingRate = 0m;
                        }
                    }
                }

                // Add opening balance row.
                model.Rows.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = openingDate,
                        GRNNumber = "Opening",
                        ProductTitle = "Opening Balance",

                        PurchaseQuantity = 0m,
                        PurchaseRate = 0m,

                        SaleQuantity = 0m,
                        SaleRate = 0m,

                        DamageQuantity = 0m,
                        DamageRate = 0m,

                        BalanceQuantity = openingQuantity,
                        BalanceRate = openingRate
                    });
            }

            // ========================================================
            // STEP 6: FILTER PURCHASES BY DATE
            // ========================================================

            var filteredPurchases = allPurchases
                .Where(x =>
                    (!fromDate.HasValue ||
                     x.Date.Date >= fromDate.Value.Date) &&
                    (!toDate.HasValue ||
                     x.Date.Date <= toDate.Value.Date))
                .ToList();

            // ========================================================
            // STEP 7: FILTER SALES BY DATE
            // ========================================================

            var filteredSales = allSales
                .Where(x =>
                    (!fromDate.HasValue ||
                     x.Date.Date >= fromDate.Value.Date) &&
                    (!toDate.HasValue ||
                     x.Date.Date <= toDate.Value.Date))
                .ToList();

            // ========================================================
            // STEP 8: FILTER DAMAGES BY DATE
            // ========================================================

            var filteredDamages = allDamages
                .Where(x =>
                    (!fromDate.HasValue ||
                     x.Date.Date >= fromDate.Value.Date) &&
                    (!toDate.HasValue ||
                     x.Date.Date <= toDate.Value.Date))
                .ToList();

            // ========================================================
            // STEP 9: COMBINE TRANSACTIONS
            // ========================================================

            var transactions =
                new List<ProductStatementRowViewModel>();

            // PURCHASE ROWS
            foreach (var purchase in filteredPurchases)
            {
                transactions.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = purchase.Date,

                        GRNNumber =
                            string.IsNullOrWhiteSpace(purchase.GRNNumber)
                                ? "-"
                                : purchase.GRNNumber,

                        ProductTitle =
                            string.IsNullOrWhiteSpace(purchase.ProductTitle)
                                ? "-"
                                : purchase.ProductTitle,

                        PurchaseQuantity = purchase.Quantity,
                        PurchaseRate = purchase.Rate,

                        SaleQuantity = 0m,
                        SaleRate = 0m,

                        DamageQuantity = 0m,
                        DamageRate = 0m
                    });
            }

            // SALE ROWS
            foreach (var sale in filteredSales)
            {
                transactions.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = sale.Date,

                        GRNNumber = "-",

                        ProductTitle =
                            string.IsNullOrWhiteSpace(sale.ProductTitle)
                                ? "-"
                                : sale.ProductTitle,

                        PurchaseQuantity = 0m,
                        PurchaseRate = 0m,

                        SaleQuantity = sale.Quantity,
                        SaleRate = sale.Rate,

                        DamageQuantity = 0m,
                        DamageRate = 0m
                    });
            }

            // DAMAGE ROWS
            foreach (var damage in filteredDamages)
            {
                transactions.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = damage.Date,

                        GRNNumber =
                            string.IsNullOrWhiteSpace(damage.DamageNumber)
                                ? "-"
                                : damage.DamageNumber,

                        ProductTitle =
                            string.IsNullOrWhiteSpace(damage.ProductTitle)
                                ? "-"
                                : damage.ProductTitle,

                        PurchaseQuantity = 0m,
                        PurchaseRate = 0m,

                        SaleQuantity = 0m,
                        SaleRate = 0m,

                        DamageQuantity = damage.Quantity,
                        DamageRate = damage.Rate
                    });
            }

            // ========================================================
            // STEP 10: SORT TRANSACTIONS
            // Purchase → Sale → Damage on the same date
            // ========================================================

            transactions = transactions
                .OrderBy(x => x.Date)
                .ThenBy(x =>
                {
                    if (x.PurchaseQuantity > 0)
                        return 0;

                    if (x.SaleQuantity > 0)
                        return 1;

                    if (x.DamageQuantity > 0)
                        return 2;

                    return 3;
                })
                .ToList();

            // ========================================================
            // STEP 11: CALCULATE RUNNING BALANCE
            // ========================================================

            decimal balanceQuantity = openingQuantity;
            decimal balanceRate = openingRate;

            foreach (var transaction in transactions)
            {
                // PURCHASE
                if (transaction.PurchaseQuantity > 0)
                {
                    decimal previousValue =
                        balanceQuantity * balanceRate;

                    decimal purchaseValue =
                        transaction.PurchaseQuantity *
                        transaction.PurchaseRate;

                    balanceQuantity += transaction.PurchaseQuantity;

                    if (balanceQuantity > 0)
                    {
                        balanceRate =
                            (previousValue + purchaseValue)
                            / balanceQuantity;
                    }
                }

                // SALE
                if (transaction.SaleQuantity > 0)
                {
                    balanceQuantity -= transaction.SaleQuantity;

                    if (balanceQuantity <= 0)
                    {
                        balanceQuantity = 0m;
                        balanceRate = 0m;
                    }
                }

                // DAMAGE
                if (transaction.DamageQuantity > 0)
                {
                    balanceQuantity -= transaction.DamageQuantity;

                    if (balanceQuantity <= 0)
                    {
                        balanceQuantity = 0m;
                        balanceRate = 0m;
                    }
                }

                transaction.BalanceQuantity = balanceQuantity;
                transaction.BalanceRate = balanceRate;
            }

            // ========================================================
            // STEP 12: ADD ROWS TO VIEW MODEL
            // ========================================================

            model.Rows.AddRange(transactions);

            return View(model);
        }

        // ============================================================
        // OPENING TRANSACTION
        // ============================================================

        private class OpeningTransaction
        {
            public DateTime Date { get; set; }

            public TransactionType TransactionType { get; set; }

            public decimal Quantity { get; set; }

            public decimal Rate { get; set; }
        }

        // ============================================================
        // TRANSACTION TYPES
        // ============================================================

        private enum TransactionType
        {
            Purchase = 0,
            Sale = 1,
            Damage = 2
        }
    }
}
