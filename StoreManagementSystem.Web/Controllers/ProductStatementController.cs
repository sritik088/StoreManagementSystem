using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.ProductStatement;

namespace StoreManagementSystem.Web.Controllers
{
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
            // VALIDATE DATE RANGE
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
            // STEP 1
            // GET ALL PURCHASE TRANSACTIONS
            // ========================================================

            var purchaseQuery = _context.GoodsReceiptItems
                .AsNoTracking()
                .Include(x => x.GoodsReceipt)
                .Include(x => x.Product)
                    .ThenInclude(x => x.SubCategory)
                .Where(x =>
                    x.GoodsReceipt != null);

            var allPurchases = await purchaseQuery
                .Select(x => new
                {
                    Date = x.GoodsReceipt!.ReceiptDate,

                    GRNNumber = x.GoodsReceipt.GRNNumber,

                    Subcategory =
                        x.Product != null &&
                        x.Product.SubCategory != null
                            ? x.Product.SubCategory.Name
                            : "-",

                    Quantity = x.ReceivedQuantity,

                    Rate = x.UnitPrice
                })
                .ToListAsync();

            // ========================================================
            // STEP 2
            // GET ALL SALES TRANSACTIONS
            // ========================================================

            var salesQuery = _context.SalesOrderItems
                .AsNoTracking()
                .Include(x => x.SalesOrder)
                .Include(x => x.Product)
                    .ThenInclude(x => x.SubCategory)
                .AsQueryable();

            var allSales = await salesQuery
                .Select(x => new
                {
                    Date = x.SalesOrder.OrderDate,

                    Subcategory =
                        x.Product != null &&
                        x.Product.SubCategory != null
                            ? x.Product.SubCategory.Name
                            : "-",

                    Quantity = x.Quantity,

                    Rate = x.UnitPrice
                })
                .ToListAsync();

            // ========================================================
            // STEP 3
            // GET ALL DAMAGE TRANSACTIONS
            // ========================================================

            var damageQuery = _context.DamageEntries
                .AsNoTracking()
                .Include(x => x.Damage)
                .Include(x => x.Product)
                    .ThenInclude(x => x.SubCategory)
                .Where(x =>
                    !x.IsDeleted);

            var allDamages = await damageQuery
                .Select(x => new
                {
                    Date = x.DamageDate,

                    DamageNumber = x.DamageNumber,

                    DamageName =
                        x.Damage != null
                            ? x.Damage.DamageName
                            : "-",

                    Subcategory =
                        x.Product != null &&
                        x.Product.SubCategory != null
                            ? x.Product.SubCategory.Name
                            : "-",

                    Quantity = x.Quantity,

                    Rate = x.UnitCost
                })
                .ToListAsync();

            // ========================================================
            // STEP 4
            // CALCULATE OPENING BALANCE
            // ========================================================

            decimal openingQuantity = 0m;
            decimal openingRate = 0m;

            if (fromDate.HasValue)
            {
                DateTime openingDate =
                    fromDate.Value.Date;

                // ----------------------------------------------------
                // PURCHASES BEFORE FROM DATE
                // ----------------------------------------------------

                var openingPurchases = allPurchases
                    .Where(x =>
                        x.Date.Date < openingDate)
                    .OrderBy(x => x.Date)
                    .ToList();

                // ----------------------------------------------------
                // SALES BEFORE FROM DATE
                // ----------------------------------------------------

                var openingSales = allSales
                    .Where(x =>
                        x.Date.Date < openingDate)
                    .OrderBy(x => x.Date)
                    .ToList();

                // ----------------------------------------------------
                // DAMAGES BEFORE FROM DATE
                // ----------------------------------------------------

                var openingDamages = allDamages
                    .Where(x =>
                        x.Date.Date < openingDate)
                    .OrderBy(x => x.Date)
                    .ToList();

                // ----------------------------------------------------
                // CREATE OPENING TRANSACTIONS
                // ----------------------------------------------------

                var openingTransactions =
                    new List<OpeningTransaction>();

                foreach (var purchase in openingPurchases)
                {
                    openingTransactions.Add(
                        new OpeningTransaction
                        {
                            Date = purchase.Date,

                            TransactionType =
                                TransactionType.Purchase,

                            Quantity =
                                purchase.Quantity,

                            Rate =
                                purchase.Rate
                        });
                }

                foreach (var sale in openingSales)
                {
                    openingTransactions.Add(
                        new OpeningTransaction
                        {
                            Date = sale.Date,

                            TransactionType =
                                TransactionType.Sale,

                            Quantity =
                                sale.Quantity,

                            Rate =
                                sale.Rate
                        });
                }

                foreach (var damage in openingDamages)
                {
                    openingTransactions.Add(
                        new OpeningTransaction
                        {
                            Date = damage.Date,

                            TransactionType =
                                TransactionType.Damage,

                            Quantity =
                                damage.Quantity,

                            Rate =
                                damage.Rate
                        });
                }

                // ----------------------------------------------------
                // SORT OPENING TRANSACTIONS
                // Purchase → Sale → Damage
                // ----------------------------------------------------

                openingTransactions = openingTransactions
                    .OrderBy(x => x.Date)
                    .ThenBy(x =>
                        x.TransactionType)
                    .ToList();

                // ----------------------------------------------------
                // CALCULATE OPENING STOCK
                // ----------------------------------------------------

                foreach (var transaction in openingTransactions)
                {
                    // =================================================
                    // PURCHASE
                    // =================================================

                    if (transaction.TransactionType ==
                        TransactionType.Purchase)
                    {
                        decimal previousValue =
                            openingQuantity *
                            openingRate;

                        decimal purchaseValue =
                            transaction.Quantity *
                            transaction.Rate;

                        openingQuantity +=
                            transaction.Quantity;

                        if (openingQuantity > 0)
                        {
                            openingRate =
                                (previousValue + purchaseValue)
                                / openingQuantity;
                        }
                    }

                    // =================================================
                    // SALE
                    // =================================================

                    else if (transaction.TransactionType ==
                             TransactionType.Sale)
                    {
                        openingQuantity -=
                            transaction.Quantity;

                        if (openingQuantity <= 0)
                        {
                            openingQuantity = 0m;
                            openingRate = 0m;
                        }
                    }

                    // =================================================
                    // DAMAGE
                    // =================================================

                    else if (transaction.TransactionType ==
                             TransactionType.Damage)
                    {
                        openingQuantity -=
                            transaction.Quantity;

                        if (openingQuantity <= 0)
                        {
                            openingQuantity = 0m;
                            openingRate = 0m;
                        }
                    }
                }
            }

            // ========================================================
            // STEP 5
            // ADD OPENING ROW FIRST
            // ========================================================

            if (fromDate.HasValue)
            {
                model.Rows.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = fromDate.Value.Date,

                        GRNNumber = "Opening",

                        Subcategory = "Opening Balance",

                        PurchaseQuantity = 0m,
                        PurchaseRate = 0m,

                        SaleQuantity = 0m,
                        SaleRate = 0m,

                        DamageQuantity = 0m,
                        DamageRate = 0m,

                        BalanceQuantity =
                            openingQuantity,

                        BalanceRate =
                            openingRate
                    });
            }

            // ========================================================
            // STEP 6
            // FILTER PURCHASE TRANSACTIONS
            // ========================================================

            var filteredPurchases =
                allPurchases.AsEnumerable();

            if (fromDate.HasValue)
            {
                filteredPurchases =
                    filteredPurchases.Where(x =>
                        x.Date.Date >=
                        fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                filteredPurchases =
                    filteredPurchases.Where(x =>
                        x.Date.Date <=
                        toDate.Value.Date);
            }

            // ========================================================
            // STEP 7
            // FILTER SALES TRANSACTIONS
            // ========================================================

            var filteredSales =
                allSales.AsEnumerable();

            if (fromDate.HasValue)
            {
                filteredSales =
                    filteredSales.Where(x =>
                        x.Date.Date >=
                        fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                filteredSales =
                    filteredSales.Where(x =>
                        x.Date.Date <=
                        toDate.Value.Date);
            }

            // ========================================================
            // STEP 8
            // FILTER DAMAGE TRANSACTIONS
            // ========================================================

            var filteredDamages =
                allDamages.AsEnumerable();

            if (fromDate.HasValue)
            {
                filteredDamages =
                    filteredDamages.Where(x =>
                        x.Date.Date >=
                        fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                filteredDamages =
                    filteredDamages.Where(x =>
                        x.Date.Date <=
                        toDate.Value.Date);
            }

            // ========================================================
            // STEP 9
            // COMBINE TRANSACTIONS
            // ========================================================

            var transactions =
                new List<ProductStatementRowViewModel>();

            // --------------------------------------------------------
            // PURCHASE ROWS
            // --------------------------------------------------------

            foreach (var purchase in filteredPurchases)
            {
                transactions.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = purchase.Date,

                        GRNNumber =
                            string.IsNullOrWhiteSpace(
                                purchase.GRNNumber)
                                ? "-"
                                : purchase.GRNNumber,

                        Subcategory =
                            purchase.Subcategory,

                        PurchaseQuantity =
                            purchase.Quantity,

                        PurchaseRate =
                            purchase.Rate,

                        SaleQuantity = 0m,
                        SaleRate = 0m,

                        DamageQuantity = 0m,
                        DamageRate = 0m
                    });
            }

            // --------------------------------------------------------
            // SALE ROWS
            // --------------------------------------------------------

            foreach (var sale in filteredSales)
            {
                transactions.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = sale.Date,

                        GRNNumber = "-",

                        Subcategory =
                            sale.Subcategory,

                        PurchaseQuantity = 0m,
                        PurchaseRate = 0m,

                        SaleQuantity =
                            sale.Quantity,

                        SaleRate =
                            sale.Rate,

                        DamageQuantity = 0m,
                        DamageRate = 0m
                    });
            }

            // --------------------------------------------------------
            // DAMAGE ROWS
            // --------------------------------------------------------

            foreach (var damage in filteredDamages)
            {
                transactions.Add(
                    new ProductStatementRowViewModel
                    {
                        Date = damage.Date,

                        // Show Damage Number in the reference column
                        GRNNumber =
                            string.IsNullOrWhiteSpace(
                                damage.DamageNumber)
                                ? "-"
                                : damage.DamageNumber,

                        Subcategory =
                            damage.Subcategory,

                        PurchaseQuantity = 0m,
                        PurchaseRate = 0m,

                        SaleQuantity = 0m,
                        SaleRate = 0m,

                        DamageQuantity =
                            damage.Quantity,

                        DamageRate =
                            damage.Rate
                    });
            }

            // ========================================================
            // STEP 10
            // SORT TRANSACTIONS
            //
            // Same date order:
            // Purchase → Sale → Damage
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
            // STEP 11
            // CONTINUE BALANCE FROM OPENING
            // ========================================================

            decimal balanceQuantity =
                openingQuantity;

            decimal balanceRate =
                openingRate;

            foreach (var transaction in transactions)
            {
                // =================================================
                // PURCHASE
                // =================================================

                if (transaction.PurchaseQuantity > 0)
                {
                    decimal previousValue =
                        balanceQuantity *
                        balanceRate;

                    decimal purchaseValue =
                        transaction.PurchaseQuantity *
                        transaction.PurchaseRate;

                    balanceQuantity +=
                        transaction.PurchaseQuantity;

                    if (balanceQuantity > 0)
                    {
                        balanceRate =
                            (previousValue + purchaseValue)
                            / balanceQuantity;
                    }
                }

                // =================================================
                // SALE
                // =================================================

                if (transaction.SaleQuantity > 0)
                {
                    balanceQuantity -=
                        transaction.SaleQuantity;

                    if (balanceQuantity <= 0)
                    {
                        balanceQuantity = 0m;
                        balanceRate = 0m;
                    }
                }

                // =================================================
                // DAMAGE
                // =================================================

                if (transaction.DamageQuantity > 0)
                {
                    balanceQuantity -=
                        transaction.DamageQuantity;

                    if (balanceQuantity <= 0)
                    {
                        balanceQuantity = 0m;
                        balanceRate = 0m;
                    }
                }

                // =================================================
                // SET RUNNING BALANCE
                // =================================================

                transaction.BalanceQuantity =
                    balanceQuantity;

                transaction.BalanceRate =
                    balanceRate;
            }

            // ========================================================
            // STEP 12
            // ADD TRANSACTIONS TO MODEL
            // ========================================================

            model.Rows.AddRange(transactions);

            return View(model);
        }

        // ============================================================
        // INTERNAL OPENING TRANSACTION
        // ============================================================

        private class OpeningTransaction
        {
            public DateTime Date { get; set; }

            public TransactionType TransactionType { get; set; }

            public decimal Quantity { get; set; }

            public decimal Rate { get; set; }
        }

        // ============================================================
        // TRANSACTION TYPE
        // ============================================================

        private enum TransactionType
        {
            Purchase = 0,
            Sale = 1,
            Damage = 2
        }
    }
}