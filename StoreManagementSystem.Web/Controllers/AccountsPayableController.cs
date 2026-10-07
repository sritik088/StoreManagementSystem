using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.AccountsPayable;

namespace StoreManagementSystem.Web.Controllers
{
    public class AccountsPayableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsPayableController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. AP DASHBOARD
        // =========================================================

        [HttpGet]
        [Route("AccountsPayable")]
        public async Task<IActionResult> Index()
        {
            var purchaseData =
                await GetSupplierPurchaseDataAsync();

            var poData =
                await GetSupplierPODataAsync();

            var paymentData =
                await _context.SupplierPayments
                    .AsNoTracking()
                    .Where(x => !x.IsCancelled)
                    .GroupBy(x => x.SupplierId)
                    .Select(g => new
                    {
                        SupplierId = g.Key,

                        TotalPayments =
                            g.Sum(x => x.Amount),

                        LastPaymentDate =
                            g.Max(x =>
                                (DateTime?)x.PaymentDate)
                    })
                    .ToListAsync();

            var paymentDictionary =
                paymentData.ToDictionary(x => x.SupplierId);

            var suppliers =
                await _context.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            var summaries =
                suppliers.Select(s =>
                {
                    purchaseData.TryGetValue(
                        s.Id,
                        out var purchase);

                    poData.TryGetValue(
                        s.Id,
                        out var po);

                    paymentDictionary.TryGetValue(
                        s.Id,
                        out var payment);

                    // -------------------------------------------------
                    // PURCHASE ORDER VALUE
                    // -------------------------------------------------

                    var purchaseOrderValue =
                        po?.TotalPOValue ?? 0m;

                    // -------------------------------------------------
                    // GOODS RECEIVED VALUE
                    // -------------------------------------------------

                    var goodsReceivedValue =
                        purchase?.TotalPurchases ?? 0m;

                    // -------------------------------------------------
                    // PAYMENTS MADE
                    // -------------------------------------------------

                    var paymentsMade =
                        payment?.TotalPayments ?? 0m;

                    // -------------------------------------------------
                    // PENDING PO VALUE
                    // -------------------------------------------------

                    var pendingPOValue =
                        Math.Max(
                            0m,
                            purchaseOrderValue -
                            goodsReceivedValue);

                    // -------------------------------------------------
                    // OUTSTANDING PAYABLE
                    // -------------------------------------------------

                    var outstandingPayable =
                        Math.Max(
                            0m,
                            goodsReceivedValue -
                            paymentsMade);

                    return new AccountsPayableSupplierSummaryViewModel
                    {
                        SupplierId =
                            s.Id,

                        SupplierName =
                            s.Name,

                        SupplierCode =
                            s.SupplierCode,

                        PurchaseOrderValue =
                            purchaseOrderValue,

                        PendingPOValue =
                            pendingPOValue,

                        GoodsReceivedValue =
                            goodsReceivedValue,

                        PaymentsMade =
                            paymentsMade,

                        OutstandingPayable =
                            outstandingPayable,

                        PurchaseCount =
                            purchase?.PurchaseCount ?? 0,

                        LastReceiptDate =
                            purchase?.LastPurchaseDate,

                        LastPaymentDate =
                            payment?.LastPaymentDate
                    };
                })
                .Where(x =>
                    x.PurchaseOrderValue > 0 ||
                    x.GoodsReceivedValue > 0 ||
                    x.PaymentsMade > 0)
                .OrderByDescending(x =>
                    x.OutstandingPayable)
                .ToList();

            var model =
                new AccountsPayableDashboardViewModel
                {
                    // -------------------------------------------------
                    // DASHBOARD TOTALS
                    // -------------------------------------------------

                    GoodsReceivedValue =
                        summaries.Sum(x =>
                            x.GoodsReceivedValue),

                    PaymentsMade =
                        summaries.Sum(x =>
                            x.PaymentsMade),

                    OutstandingPayable =
                        summaries.Sum(x =>
                            x.OutstandingPayable),

                    PurchaseOrderValue =
                        summaries.Sum(x =>
                            x.PurchaseOrderValue),

                    PendingPOValue =
                        summaries.Sum(x =>
                            x.PendingPOValue),

                    // -------------------------------------------------
                    // SUPPLIER COUNTS
                    // -------------------------------------------------

                    SuppliersWithOutstanding =
                        summaries.Count(x =>
                            x.OutstandingPayable > 0),

                    ActiveSuppliers =
                        summaries.Count,

                    // -------------------------------------------------
                    // SUPPLIER SUMMARY
                    // -------------------------------------------------

                    SupplierSummaries =
                        summaries
                };

            return View(model);
        }


        // =========================================================
        // 2. SUPPLIER OUTSTANDING
        // =========================================================

        [HttpGet]
        [Route("SupplierOutstanding")]
        public async Task<IActionResult> SupplierOutstanding(
            string? search)
        {
            var suppliers =
                await _context.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            var purchases =
                await GetSupplierPurchaseDataAsync();

            var payments =
                await _context.SupplierPayments
                    .AsNoTracking()
                    .Where(x => !x.IsCancelled)
                    .GroupBy(x => x.SupplierId)
                    .Select(g => new
                    {
                        SupplierId =
                            g.Key,

                        TotalPayments =
                            g.Sum(x => x.Amount)
                    })
                    .ToDictionaryAsync(
                        x => x.SupplierId,
                        x => x.TotalPayments);

            var rows =
                new List<SupplierOutstandingRowViewModel>();

            foreach (var supplier in suppliers)
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term =
                        search.Trim();

                    if (!supplier.Name.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase) &&
                        !(supplier.SupplierCode ?? "")
                            .Contains(
                                term,
                                StringComparison.OrdinalIgnoreCase) &&
                        !(supplier.Phone ?? "")
                            .Contains(
                                term,
                                StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                purchases.TryGetValue(
                    supplier.Id,
                    out var purchase);

                payments.TryGetValue(
                    supplier.Id,
                    out var paid);

                var goodsReceivedValue =
                    purchase?.TotalPurchases ?? 0m;

                var paymentsMade =
                    paid;

                var outstandingPayable =
                    Math.Max(
                        0m,
                        goodsReceivedValue -
                        paymentsMade);

                if (goodsReceivedValue == 0 &&
                    paymentsMade == 0)
                {
                    continue;
                }

                rows.Add(
                    new SupplierOutstandingRowViewModel
                    {
                        SupplierId =
                            supplier.Id,

                        SupplierName =
                            supplier.Name,

                        SupplierCode =
                            supplier.SupplierCode,

                        Phone =
                            supplier.Phone,

                        GoodsReceivedValue =
                            goodsReceivedValue,

                        PaymentsMade =
                            paymentsMade,

                        OutstandingPayable =
                            outstandingPayable
                    });
            }

            var model =
                new SupplierOutstandingViewModel
                {
                    Search =
                        search ?? string.Empty,

                    TotalOutstanding =
                        rows.Sum(x =>
                            x.OutstandingPayable),

                    Items =
                        rows
                            .OrderByDescending(x =>
                                x.OutstandingPayable)
                            .ToList()
                };

            return View(model);
        }

        // =========================================================
        // 3. SUPPLIER LEDGER
        // =========================================================

        [HttpGet]
        [Route("SupplierLedger")]
        public async Task<IActionResult> SupplierLedger(
            int? supplierId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // ---------------------------------------------------------
            // SUPPLIER DROPDOWN
            // ---------------------------------------------------------

            var suppliers =
                await _context.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            ViewBag.Suppliers = suppliers;


            // ---------------------------------------------------------
            // NO SUPPLIER SELECTED
            // ---------------------------------------------------------

            if (!supplierId.HasValue)
            {
                return View(
                    new SupplierLedgerViewModel
                    {
                        FromDate = fromDate,
                        ToDate = toDate
                    });
            }


            // ---------------------------------------------------------
            // DATE VALIDATION
            // ---------------------------------------------------------

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                ModelState.AddModelError(
                    "",
                    "Date From cannot be greater than Date To.");

                return View(
                    new SupplierLedgerViewModel
                    {
                        SupplierId = supplierId,
                        FromDate = fromDate,
                        ToDate = toDate
                    });
            }


            // ---------------------------------------------------------
            // SUPPLIER
            // ---------------------------------------------------------

            var supplier =
                await _context.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == supplierId.Value &&
                        !x.IsDeleted);

            if (supplier == null)
                return NotFound();


            var allEntries =
                new List<SupplierLedgerRowViewModel>();


            // =========================================================
            // PURCHASE GRNs
            // =========================================================

            var grns =
                await _context.GoodsReceipts
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&

                        x.ReceiptType ==
                            GoodsReceiptType.Purchase &&

                        x.Status ==
                            GoodsReceiptStatus.Received &&

                        (
                            x.SupplierId ==
                                supplierId.Value ||

                            (
                                x.SupplierId == null &&
                                x.PurchaseOrder != null &&
                                x.PurchaseOrder.SupplierId ==
                                    supplierId.Value
                            )
                        ))
                    .OrderBy(x =>
                        x.ReceiptDate)
                    .Select(x => new
                    {
                        x.ReceiptDate,
                        x.GRNNumber,
                        x.GrandTotal,
                        x.Remarks
                    })
                    .ToListAsync();


            foreach (var grn in grns)
            {
                allEntries.Add(
                    new SupplierLedgerRowViewModel
                    {
                        Date =
                            grn.ReceiptDate,

                        ReferenceNo =
                            grn.GRNNumber,

                        TransactionType =
                            "Purchase",

                        Debit =
                            grn.GrandTotal,

                        Credit =
                            0m,

                        Remarks =
                            grn.Remarks
                    });
            }


            // =========================================================
            // SUPPLIER PAYMENTS
            // =========================================================

            var payments =
                await _context.SupplierPayments
                    .AsNoTracking()
                    .Where(x =>
                        x.SupplierId ==
                            supplierId.Value &&

                        !x.IsCancelled)
                    .OrderBy(x =>
                        x.PaymentDate)
                    .Select(x => new
                    {
                        x.PaymentDate,
                        x.PaymentNumber,
                        x.Amount,
                        x.PaymentMode,
                        x.ReferenceNumber,
                        x.Remarks
                    })
                    .ToListAsync();


            foreach (var payment in payments)
            {
                allEntries.Add(
                    new SupplierLedgerRowViewModel
                    {
                        Date =
                            payment.PaymentDate,

                        ReferenceNo =
                            payment.PaymentNumber,

                        TransactionType =
                            $"Payment ({payment.PaymentMode})",

                        Debit =
                            0m,

                        Credit =
                            payment.Amount,

                        Remarks =
                            string.IsNullOrWhiteSpace(
                                payment.ReferenceNumber)
                                ? payment.Remarks
                                : payment.ReferenceNumber
                    });
            }


            // =========================================================
            // SORT ALL TRANSACTIONS
            // =========================================================

            allEntries =
                allEntries
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.ReferenceNo)
                    .ToList();


            // =========================================================
            // OPENING BALANCE
            //
            // Everything BEFORE FromDate
            // Purchase = Debit
            // Payment  = Credit
            // =========================================================

            decimal openingBalance = 0m;

            if (fromDate.HasValue)
            {
                var openingDate =
                    fromDate.Value.Date;

                openingBalance =
                    allEntries
                        .Where(x =>
                            x.Date.Date < openingDate)
                        .Sum(x =>
                            x.Debit - x.Credit);
            }


            // =========================================================
            // FILTER SELECTED PERIOD
            // =========================================================

            var entries =
                allEntries;


            // ---------------------------------------------------------
            // FROM DATE
            // ---------------------------------------------------------

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;

                entries =
                    entries
                        .Where(x =>
                            x.Date >= from)
                        .ToList();
            }


            // ---------------------------------------------------------
            // TO DATE
            // ---------------------------------------------------------

            if (toDate.HasValue)
            {
                var toExclusive =
                    toDate.Value.Date.AddDays(1);

                entries =
                    entries
                        .Where(x =>
                            x.Date < toExclusive)
                        .ToList();
            }


            // =========================================================
            // RUNNING BALANCE
            //
            // Start from Opening Balance
            // =========================================================

            decimal balance =
                openingBalance;


            foreach (var entry in entries)
            {
                balance +=
                    entry.Debit;

                balance -=
                    entry.Credit;

                entry.Balance =
                    balance;
            }


            // =========================================================
            // SELECTED PERIOD TOTALS
            // =========================================================

            var goodsReceivedValue =
                entries.Sum(x =>
                    x.Debit);

            var paymentsMade =
                entries.Sum(x =>
                    x.Credit);


            // =========================================================
            // MODEL
            // =========================================================

            var model =
                new SupplierLedgerViewModel
                {
                    SupplierId =
                        supplier.Id,

                    SupplierName =
                        supplier.Name,

                    FromDate =
                        fromDate,

                    ToDate =
                        toDate,

                    // IMPORTANT:
                    // Balance before selected From Date
                    OpeningBalance =
                        openingBalance,

                    // Only selected period
                    GoodsReceivedValue =
                        goodsReceivedValue,

                    // Only selected period
                    PaymentsMade =
                        paymentsMade,

                    // Opening + selected period
                    ClosingOutstanding =
                        Math.Max(
                            0m,
                            balance),

                    Entries =
                        entries
                };


            return View(model);
        }

        // =========================================================
        // 3A. SUPPLIER LEDGER PDF
        // =========================================================

        [HttpGet]
        [Route("SupplierLedger/Pdf")]
        public async Task<IActionResult> SupplierLedgerPdf(
            int supplierId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (supplierId <= 0)
                return BadRequest("Invalid supplier.");

            var supplier =
                await _context.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == supplierId &&
                        !x.IsDeleted);

            if (supplier == null)
                return NotFound("Supplier not found.");

            // ---------------------------------------------------------
            // PURCHASE GRNs
            // ---------------------------------------------------------

            var grns =
                await _context.GoodsReceipts
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&

                        x.ReceiptType ==
                            GoodsReceiptType.Purchase &&

                        x.Status ==
                            GoodsReceiptStatus.Received &&

                        (
                            x.SupplierId ==
                                supplierId ||

                            (
                                x.SupplierId == null &&
                                x.PurchaseOrder != null &&
                                x.PurchaseOrder.SupplierId ==
                                    supplierId
                            )
                        ))
                    .Select(x => new
                    {
                        x.ReceiptDate,
                        x.GRNNumber,
                        x.GrandTotal,
                        x.Remarks
                    })
                    .ToListAsync();

            var entries =
                new List<SupplierLedgerRowViewModel>();

            foreach (var grn in grns)
            {
                entries.Add(
                    new SupplierLedgerRowViewModel
                    {
                        Date = grn.ReceiptDate,
                        ReferenceNo = grn.GRNNumber,
                        TransactionType = "Purchase",
                        Debit = grn.GrandTotal,
                        Credit = 0m,
                        Remarks = grn.Remarks
                    });
            }

            // ---------------------------------------------------------
            // PAYMENTS
            // ---------------------------------------------------------

            var payments =
                await _context.SupplierPayments
                    .AsNoTracking()
                    .Where(x =>
                        x.SupplierId == supplierId &&
                        !x.IsCancelled)
                    .Select(x => new
                    {
                        x.PaymentDate,
                        x.PaymentNumber,
                        x.Amount,
                        x.PaymentMode,
                        x.ReferenceNumber,
                        x.Remarks
                    })
                    .ToListAsync();

            foreach (var payment in payments)
            {
                entries.Add(
                    new SupplierLedgerRowViewModel
                    {
                        Date = payment.PaymentDate,
                        ReferenceNo = payment.PaymentNumber,
                        TransactionType =
                            $"Payment ({payment.PaymentMode})",
                        Debit = 0m,
                        Credit = payment.Amount,
                        Remarks =
                            string.IsNullOrWhiteSpace(
                                payment.ReferenceNumber)
                                ? payment.Remarks
                                : payment.ReferenceNumber
                    });
            }

            // ---------------------------------------------------------
            // SORT
            // ---------------------------------------------------------

            entries =
                entries
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.ReferenceNo)
                    .ToList();

            // ---------------------------------------------------------
            // OPENING BALANCE
            // ---------------------------------------------------------

            decimal openingBalance = 0m;

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;

                openingBalance =
                    entries
                        .Where(x =>
                            x.Date.Date < from)
                        .Sum(x =>
                            x.Debit - x.Credit);
            }

            // ---------------------------------------------------------
            // DATE FILTER
            // ---------------------------------------------------------

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;

                entries =
                    entries
                        .Where(x =>
                            x.Date >= from)
                        .ToList();
            }

            if (toDate.HasValue)
            {
                var toExclusive =
                    toDate.Value.Date.AddDays(1);

                entries =
                    entries
                        .Where(x =>
                            x.Date < toExclusive)
                        .ToList();
            }

            // ---------------------------------------------------------
            // BALANCE
            // ---------------------------------------------------------

            decimal balance =
                openingBalance;

            foreach (var entry in entries)
            {
                balance += entry.Debit;
                balance -= entry.Credit;

                entry.Balance = balance;
            }

            // ---------------------------------------------------------
            // PDF
            // ---------------------------------------------------------

            QuestPDF.Settings.License =
                QuestPDF.Infrastructure.LicenseType.Community;

            var document =
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(
                            QuestPDF.Helpers.PageSizes.A4);

                        page.Margin(30);

                        page.DefaultTextStyle(
                            x => x.FontSize(9));

                        page.Header()
                            .Column(column =>
                            {
                                column.Item()
                                    .Text("SUPPLIER LEDGER")
                                    .Bold()
                                    .FontSize(18);

                                column.Item()
                                    .Text(
                                        $"Supplier: {supplier.Name}")
                                    .Bold()
                                    .FontSize(11);

                                if (fromDate.HasValue ||
                                    toDate.HasValue)
                                {
                                    column.Item()
                                        .Text(
                                            $"Period: " +
                                            $"{(fromDate.HasValue ? fromDate.Value.ToString("dd-MMM-yyyy") : "Beginning")} " +
                                            $"to " +
                                            $"{(toDate.HasValue ? toDate.Value.ToString("dd-MMM-yyyy") : "Present")}");
                                }

                                column.Item()
                                    .PaddingTop(5)
                                    .LineHorizontal(1);
                            });

                        page.Content()
                            .PaddingTop(15)
                            .Column(column =>
                            {
                                // SUMMARY

                                column.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text(
                                                $"Opening Balance: ₹{openingBalance:N2}");

                                        row.RelativeItem()
                                            .Text(
                                                $"Purchases: ₹{entries.Sum(x => x.Debit):N2}");

                                        row.RelativeItem()
                                            .Text(
                                                $"Payments: ₹{entries.Sum(x => x.Credit):N2}");

                                        row.RelativeItem()
                                            .Text(
                                                $"Closing: ₹{Math.Max(0m, balance):N2}");
                                    });

                                column.Item()
                                    .PaddingTop(15);

                                // TABLE

                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(
                                            columns =>
                                            {
                                                columns.ConstantColumn(65);
                                                columns.RelativeColumn(1.4f);
                                                columns.RelativeColumn(1.3f);
                                                columns.ConstantColumn(70);
                                                columns.ConstantColumn(70);
                                                columns.ConstantColumn(75);
                                            });

                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Element(HeaderCell)
                                                .Text("Date");

                                            header.Cell()
                                                .Element(HeaderCell)
                                                .Text("Reference");

                                            header.Cell()
                                                .Element(HeaderCell)
                                                .Text("Transaction");

                                            header.Cell()
                                                .Element(HeaderCell)
                                                .AlignRight()
                                                .Text("Debit");

                                            header.Cell()
                                                .Element(HeaderCell)
                                                .AlignRight()
                                                .Text("Credit");

                                            header.Cell()
                                                .Element(HeaderCell)
                                                .AlignRight()
                                                .Text("Balance");
                                        });

                                        foreach (var entry in entries)
                                        {
                                            table.Cell()
                                                .Element(BodyCell)
                                                .Text(
                                                    entry.Date.ToString(
                                                        "dd-MMM-yyyy"));

                                            table.Cell()
                                                .Element(BodyCell)
                                                .Text(
                                                    entry.ReferenceNo);

                                            table.Cell()
                                                .Element(BodyCell)
                                                .Text(
                                                    entry.TransactionType);

                                            table.Cell()
                                                .Element(BodyCell)
                                                .AlignRight()
                                                .Text(
                                                    entry.Debit > 0
                                                        ? $"₹{entry.Debit:N2}"
                                                        : "-");

                                            table.Cell()
                                                .Element(BodyCell)
                                                .AlignRight()
                                                .Text(
                                                    entry.Credit > 0
                                                        ? $"₹{entry.Credit:N2}"
                                                        : "-");

                                            table.Cell()
                                                .Element(BodyCell)
                                                .AlignRight()
                                                .Text(
                                                    $"₹{entry.Balance:N2}");
                                        }
                                    });

                                column.Item()
                                    .PaddingTop(15)
                                    .AlignRight()
                                    .Text(
                                        $"Closing Outstanding: ₹{Math.Max(0m, balance):N2}")
                                    .Bold()
                                    .FontSize(11);
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(
                                $"Generated on {DateTime.Now:dd-MMM-yyyy HH:mm}");
                    });
                });

            var pdfBytes =
                document.GeneratePdf();

            var fileName =
                $"SupplierLedger-{supplier.Name.Replace(" ", "-")}-{DateTime.Now:yyyyMMddHHmmss}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName);
        }

        // =========================================================
        // PDF TABLE HELPERS
        // =========================================================

        private static QuestPDF.Infrastructure.IContainer HeaderCell(
            QuestPDF.Infrastructure.IContainer container)
        {
            return container
                .Background(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Border(1)
                .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten1)
                .Padding(5);
        }

        private static QuestPDF.Infrastructure.IContainer BodyCell(
            QuestPDF.Infrastructure.IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                .Padding(5);
        }

        // =========================================================
        // 4. SUPPLIER PAYMENTS
        // =========================================================

        [HttpGet]
        [Route("SupplierPayments")]
        public async Task<IActionResult> SupplierPayments(
            string? search,
            int? supplierId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query =
                _context.SupplierPayments
                    .AsNoTracking()
                    .Include(x => x.Supplier)
                    .AsQueryable();

            if (supplierId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.SupplierId ==
                        supplierId.Value);
            }

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;

                query =
                    query.Where(x =>
                        x.PaymentDate >= from);
            }

            if (toDate.HasValue)
            {
                var toExclusive =
                    toDate.Value.Date.AddDays(1);

                query =
                    query.Where(x =>
                        x.PaymentDate <
                        toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term =
                    search.Trim();

                query =
                    query.Where(x =>
                        x.PaymentNumber.Contains(term) ||
                        (x.Supplier != null &&
                         x.Supplier.Name.Contains(term)) ||
                        (x.ReferenceNumber != null &&
                         x.ReferenceNumber.Contains(term)));
            }

            var payments =
                await query
                    .OrderByDescending(x =>
                        x.PaymentDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .ToListAsync();

            var model =
                new SupplierPaymentsViewModel
                {
                    Search =
                        search ?? string.Empty,

                    SupplierId =
                        supplierId,

                    FromDate =
                        fromDate,

                    ToDate =
                        toDate,

                    PaymentsMade =
                        payments
                            .Where(x =>
                                !x.IsCancelled)
                            .Sum(x =>
                                x.Amount),

                    Payments =
                        payments
                            .Select(x =>
                                new SupplierPaymentRowViewModel
                                {
                                    Id =
                                        x.Id,

                                    PaymentNumber =
                                        x.PaymentNumber,

                                    PaymentDate =
                                        x.PaymentDate,

                                    SupplierName =
                                        x.Supplier?.Name
                                        ?? "-",

                                    Amount =
                                        x.Amount,

                                    PaymentMode =
                                        x.PaymentMode,

                                    ReferenceNumber =
                                        x.ReferenceNumber,

                                    BankName =
                                        x.BankName,

                                    Remarks =
                                        x.Remarks,

                                    IsCancelled =
                                        x.IsCancelled,

                                    CancellationRemarks =
                                        x.CancellationRemarks
                                })
                            .ToList()
                };

            ViewBag.Suppliers =
                await _context.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            return View(model);
        }


        // =========================================================
        // 5. CREATE PAYMENT - GET
        // =========================================================

        [HttpGet]
        [Route("SupplierPayments/Create")]
        public async Task<IActionResult> CreatePayment()
        {
            var model =
                await BuildPaymentCreateModelAsync();

            return View(
                "Create",
                model);
        }


        // =========================================================
        // 6. CREATE PAYMENT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("SupplierPayments/Create")]
        public async Task<IActionResult> CreatePayment(
            SupplierPaymentCreateViewModel model)
        {
            await LoadSupplierOptionsAsync(model);

            if (!ModelState.IsValid)
                return View("Create", model);

            var outstanding =
                await GetSupplierOutstandingAsync(
                    model.SupplierId);

            model.CurrentOutstanding =
                outstanding;

            if (outstanding <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.Amount),
                    "This supplier has no outstanding payable amount. " +
                    "First create/receive a Purchase GRN for this supplier.");

                return View(
                    "Create",
                    model);
            }

            if (model.Amount > outstanding)
            {
                ModelState.AddModelError(
                    nameof(model.Amount),
                    $"Payment cannot exceed the current outstanding amount of ₹{outstanding:N2}.");

                return View(
                    "Create",
                    model);
            }

            var paymentNumber =
                await GeneratePaymentNumberAsync();

            var payment =
                new SupplierPayment
                {
                    PaymentNumber =
                        paymentNumber,

                    SupplierId =
                        model.SupplierId,

                    PaymentDate =
                        model.PaymentDate,

                    Amount =
                        model.Amount,

                    PaymentMode =
                        model.PaymentMode,

                    ReferenceNumber =
                        model.ReferenceNumber,

                    BankName =
                        model.BankName,

                    Remarks =
                        model.Remarks,

                    CreatedBy =
                        User.Identity?.Name
                        ?? "System",

                    CreatedDate =
                        DateTime.Now,

                    IsCancelled =
                        false
                };

            _context.SupplierPayments.Add(
                payment);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Supplier payment {paymentNumber} recorded successfully.";

            return RedirectToAction(
                nameof(SupplierPayments));
        }


        // =========================================================
        // 7. GET OUTSTANDING FOR SELECTED SUPPLIER
        // =========================================================

        [HttpGet]
        [Route("SupplierPayments/GetOutstanding/{supplierId:int}")]
        public async Task<IActionResult> GetOutstanding(
            int supplierId)
        {
            if (supplierId <= 0)
            {
                return Json(new
                {
                    success = false,
                    goodsReceivedValue = 0m,
                    paymentsMade = 0m,
                    outstandingPayable = 0m,
                    message = "Invalid supplier."
                });
            }

            var supplierExists =
                await _context.Suppliers
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == supplierId &&
                        !x.IsDeleted &&
                        x.IsActive);

            if (!supplierExists)
            {
                return Json(new
                {
                    success = false,
                    goodsReceivedValue = 0m,
                    paymentsMade = 0m,
                    outstandingPayable = 0m,
                    message = "Supplier not found."
                });
            }

            var goodsReceivedValue =
                await GetSupplierPurchaseTotalAsync(
                    supplierId);

            var paymentsMade =
                await GetSupplierPaymentTotalAsync(
                    supplierId);

            var outstandingPayable =
                Math.Max(
                    0m,
                    goodsReceivedValue -
                    paymentsMade);

            return Json(new
            {
                success = true,

                goodsReceivedValue =
                    goodsReceivedValue,

                paymentsMade =
                    paymentsMade,

                outstandingPayable =
                    outstandingPayable
            });
        }


        // =========================================================
        // 8. CANCEL PAYMENT - GET
        // =========================================================

        [HttpGet]
        [Route("SupplierPayments/Cancel/{id:int}")]
        public async Task<IActionResult> CancelPayment(
            int id)
        {
            var payment =
                await _context.SupplierPayments
                    .AsNoTracking()
                    .Include(x => x.Supplier)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (payment == null)
                return NotFound();

            if (payment.IsCancelled)
            {
                TempData["Error"] =
                    "This payment is already cancelled.";

                return RedirectToAction(
                    nameof(SupplierPayments));
            }

            var model =
                new SupplierPaymentCancelViewModel
                {
                    Id =
                        payment.Id,

                    PaymentNumber =
                        payment.PaymentNumber,

                    SupplierName =
                        payment.Supplier?.Name
                        ?? "-",

                    Amount =
                        payment.Amount
                };

            return View(
                "Cancel",
                model);
        }


        // =========================================================
        // 9. CANCEL PAYMENT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("SupplierPayments/Cancel/{id:int}")]
        public async Task<IActionResult> CancelPayment(
            int id,
            SupplierPaymentCancelViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(
                    "Cancel",
                    model);

            var payment =
                await _context.SupplierPayments
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (payment == null)
                return NotFound();

            if (payment.IsCancelled)
            {
                TempData["Error"] =
                    "This payment is already cancelled.";

                return RedirectToAction(
                    nameof(SupplierPayments));
            }

            payment.IsCancelled =
                true;

            payment.CancelledDate =
                DateTime.Now;

            payment.CancellationRemarks =
                model.CancellationRemarks;

            payment.UpdatedDate =
                DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Payment {payment.PaymentNumber} cancelled successfully. " +
                "The supplier outstanding has been restored.";

            return RedirectToAction(
                nameof(SupplierPayments));
        }


        // =========================================================
        // 10. SUPPLIER PURCHASE / GRN DATA
        // =========================================================

        private async Task<
            Dictionary<int, SupplierPurchaseSummary>>
            GetSupplierPurchaseDataAsync()
        {
            var grns =
                await _context.GoodsReceipts
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&

                        x.ReceiptType ==
                            GoodsReceiptType.Purchase &&

                        x.Status ==
                            GoodsReceiptStatus.Received &&

                        (
                            x.SupplierId.HasValue ||

                            x.PurchaseOrder != null
                        ))
                    .Select(x => new
                    {
                        SupplierId =
                            x.SupplierId
                            ??
                            (
                                x.PurchaseOrder != null
                                    ? x.PurchaseOrder.SupplierId
                                    : 0
                            ),

                        x.GrandTotal,
                        x.ReceiptDate
                    })
                    .Where(x =>
                        x.SupplierId > 0)
                    .ToListAsync();

            var data =
                grns
                    .GroupBy(x =>
                        x.SupplierId)
                    .Select(g => new
                    {
                        SupplierId =
                            g.Key,

                        TotalPurchases =
                            g.Sum(x =>
                                x.GrandTotal),

                        PurchaseCount =
                            g.Count(),

                        LastPurchaseDate =
                            g.Max(x =>
                                (DateTime?)
                                    x.ReceiptDate)
                    })
                    .ToList();

            return data.ToDictionary(
                x => x.SupplierId,
                x => new SupplierPurchaseSummary
                {
                    TotalPurchases =
                        x.TotalPurchases,

                    PurchaseCount =
                        x.PurchaseCount,

                    LastPurchaseDate =
                        x.LastPurchaseDate
                });
        }


        // =========================================================
        // 11. SUPPLIER PO DATA
        // =========================================================

        private async Task<
            Dictionary<int, SupplierPOSummary>>
            GetSupplierPODataAsync()
        {
            var data =
                await _context.PurchaseOrders
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.SupplierId > 0 &&
                        x.Status !=
                            PurchaseOrderStatus.Cancelled)
                    .GroupBy(x =>
                        x.SupplierId)
                    .Select(g => new
                    {
                        SupplierId =
                            g.Key,

                        TotalPOValue =
                            g.Sum(x =>
                                x.GrandTotal)
                    })
                    .ToListAsync();

            return data.ToDictionary(
                x => x.SupplierId,
                x => new SupplierPOSummary
                {
                    TotalPOValue =
                        x.TotalPOValue
                });
        }


        // =========================================================
        // 12. SUPPLIER OUTSTANDING
        // =========================================================

        private async Task<decimal>
            GetSupplierOutstandingAsync(
                int supplierId)
        {
            var goodsReceivedValue =
                await GetSupplierPurchaseTotalAsync(
                    supplierId);

            var paymentsMade =
                await GetSupplierPaymentTotalAsync(
                    supplierId);

            return Math.Max(
                0m,
                goodsReceivedValue -
                paymentsMade);
        }


        // =========================================================
        // 13. GOODS RECEIVED VALUE
        // =========================================================

        private async Task<decimal>
            GetSupplierPurchaseTotalAsync(
                int supplierId)
        {
            if (supplierId <= 0)
                return 0m;

            var total =
                await _context.GoodsReceipts
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&

                        x.ReceiptType ==
                            GoodsReceiptType.Purchase &&

                        x.Status ==
                            GoodsReceiptStatus.Received &&

                        (
                            x.SupplierId ==
                                supplierId ||

                            (
                                x.SupplierId == null &&
                                x.PurchaseOrder != null &&
                                x.PurchaseOrder.SupplierId ==
                                    supplierId
                            )
                        ))
                    .SumAsync(x =>
                        (decimal?)x.GrandTotal)
                    ?? 0m;

            return total;
        }


        // =========================================================
        // 14. PAYMENTS MADE
        // =========================================================

        private async Task<decimal>
            GetSupplierPaymentTotalAsync(
                int supplierId)
        {
            if (supplierId <= 0)
                return 0m;

            return await _context.SupplierPayments
                .AsNoTracking()
                .Where(x =>
                    x.SupplierId ==
                        supplierId &&

                    !x.IsCancelled)
                .SumAsync(x =>
                    (decimal?)x.Amount)
                ?? 0m;
        }


        // =========================================================
        // 15. PAYMENT CREATE MODEL
        // =========================================================

        private async Task<
            SupplierPaymentCreateViewModel>
            BuildPaymentCreateModelAsync()
        {
            var suppliers =
                await _context.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            var model =
                new SupplierPaymentCreateViewModel();

            model.Suppliers =
                new List<SupplierSelectItemViewModel>();

            foreach (var supplier in suppliers)
            {
                var goodsReceivedValue =
                    await GetSupplierPurchaseTotalAsync(
                        supplier.Id);

                var paymentsMade =
                    await GetSupplierPaymentTotalAsync(
                        supplier.Id);

                var outstandingPayable =
                    Math.Max(
                        0m,
                        goodsReceivedValue -
                        paymentsMade);

                model.Suppliers.Add(
                    new SupplierSelectItemViewModel
                    {
                        Id =
                            supplier.Id,

                        Name =
                            supplier.Name,

                        Outstanding =
                            outstandingPayable
                    });
            }

            return model;
        }


        // =========================================================
        // 16. LOAD SUPPLIER OPTIONS
        // =========================================================

        private async Task
            LoadSupplierOptionsAsync(
                SupplierPaymentCreateViewModel model)
        {
            var suppliers =
                await _context.Suppliers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            model.Suppliers =
                new List<SupplierSelectItemViewModel>();

            foreach (var supplier in suppliers)
            {
                var goodsReceivedValue =
                    await GetSupplierPurchaseTotalAsync(
                        supplier.Id);

                var paymentsMade =
                    await GetSupplierPaymentTotalAsync(
                        supplier.Id);

                var outstandingPayable =
                    Math.Max(
                        0m,
                        goodsReceivedValue -
                        paymentsMade);

                model.Suppliers.Add(
                    new SupplierSelectItemViewModel
                    {
                        Id =
                            supplier.Id,

                        Name =
                            supplier.Name,

                        Outstanding =
                            outstandingPayable
                    });
            }
        }


        // =========================================================
        // 17. PAYMENT NUMBER
        // =========================================================

        private async Task<string>
            GeneratePaymentNumberAsync()
        {
            var prefix =
                $"PAY-{DateTime.Now:yyyyMMdd}-";

            var lastNumber =
                await _context.SupplierPayments
                    .Where(x =>
                        x.PaymentNumber
                            .StartsWith(prefix))
                    .OrderByDescending(x =>
                        x.Id)
                    .Select(x =>
                        x.PaymentNumber)
                    .FirstOrDefaultAsync();

            var next = 1;

            if (!string.IsNullOrWhiteSpace(
                    lastNumber))
            {
                var numericPart =
                    lastNumber.Substring(
                        prefix.Length);

                if (int.TryParse(
                        numericPart,
                        out var parsed))
                {
                    next =
                        parsed + 1;
                }
            }

            return
                $"{prefix}{next:0000}";
        }


        // =========================================================
        // INTERNAL SUMMARY
        // =========================================================

        private class SupplierPurchaseSummary
        {
            public decimal TotalPurchases { get; set; }

            public int PurchaseCount { get; set; }

            public DateTime? LastPurchaseDate { get; set; }
        }

        private class SupplierPOSummary
        {
            public decimal TotalPOValue { get; set; }
        }


        // =========================================================
        // TEST GRNs
        // =========================================================

        [HttpGet]
        [Route("SupplierPayments/TestGRNs/{supplierId:int}")]
        public async Task<IActionResult> TestGRNs(
            int supplierId)
        {
            var grns =
                await _context.GoodsReceipts
                    .AsNoTracking()
                    .Include(x =>
                        x.PurchaseOrder)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ReceiptType ==
                            GoodsReceiptType.Purchase)
                    .OrderByDescending(x =>
                        x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.GRNNumber,
                        x.SupplierId,

                        PurchaseOrderId =
                            x.PurchaseOrderId,

                        POSupplierId =
                            x.PurchaseOrder != null
                                ? x.PurchaseOrder.SupplierId
                                : 0,

                        x.GrandTotal,
                        x.Status,
                        x.IsDeleted,
                        x.ReceiptType,

                        MatchesDirectSupplier =
                            x.SupplierId ==
                            supplierId,

                        MatchesPOSupplier =
                            x.SupplierId == null &&
                            x.PurchaseOrder != null &&
                            x.PurchaseOrder.SupplierId ==
                            supplierId
                    })
                    .ToListAsync();

            return Json(grns);
        }
    }
}