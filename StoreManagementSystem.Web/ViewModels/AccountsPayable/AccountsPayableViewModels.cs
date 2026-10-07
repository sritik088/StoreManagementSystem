using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.AccountsPayable
{
    // =============================================================
    // AP DASHBOARD
    // =============================================================

    public class AccountsPayableDashboardViewModel
    {
        // =========================================================
        // SUMMARY
        // =========================================================

        /// <summary>
        /// Total value of goods received through Purchase GRNs.
        /// </summary>
        public decimal GoodsReceivedValue { get; set; }

        /// <summary>
        /// Total payments made to suppliers.
        /// </summary>
        public decimal PaymentsMade { get; set; }

        /// <summary>
        /// Current outstanding payable amount.
        /// </summary>
        public decimal OutstandingPayable { get; set; }


        // =========================================================
        // PURCHASE ORDER / AP VALUES
        // =========================================================

        /// <summary>
        /// Total value of active Purchase Orders.
        /// </summary>
        public decimal PurchaseOrderValue { get; set; }

        /// <summary>
        /// Purchase Order value which is still pending to be received.
        /// </summary>
        public decimal PendingPOValue { get; set; }


        // =========================================================
        // SUPPLIER COUNTS
        // =========================================================

        /// <summary>
        /// Number of suppliers having outstanding payable balance.
        /// </summary>
        public int SuppliersWithOutstanding { get; set; }

        /// <summary>
        /// Number of active suppliers participating in AP activity.
        /// </summary>
        public int ActiveSuppliers { get; set; }


        // =========================================================
        // SUPPLIER SUMMARY
        // =========================================================

        public List<AccountsPayableSupplierSummaryViewModel>
            SupplierSummaries
        { get; set; } = new();
    }


    // =============================================================
    // SUPPLIER PAYABLE SUMMARY
    // =============================================================

    public class AccountsPayableSupplierSummaryViewModel
    {
        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public string? SupplierCode { get; set; }


        // =========================================================
        // PURCHASE ORDER
        // =========================================================

        /// <summary>
        /// Total Purchase Order value for this supplier.
        /// </summary>
        public decimal PurchaseOrderValue { get; set; }

        /// <summary>
        /// Purchase Order value which has not yet been received.
        /// </summary>
        public decimal PendingPOValue { get; set; }


        // =========================================================
        // SUPPLIER TRANSACTIONS
        // =========================================================

        /// <summary>
        /// Value of goods actually received from the supplier.
        /// </summary>
        public decimal GoodsReceivedValue { get; set; }

        /// <summary>
        /// Amount actually paid to the supplier.
        /// </summary>
        public decimal PaymentsMade { get; set; }

        /// <summary>
        /// Current outstanding payable amount.
        /// GoodsReceivedValue - PaymentsMade.
        /// </summary>
        public decimal OutstandingPayable { get; set; }


        // =========================================================
        // SUPPLIER ACTIVITY
        // =========================================================

        public int PurchaseCount { get; set; }

        public DateTime? LastReceiptDate { get; set; }

        public DateTime? LastPaymentDate { get; set; }
    }


    // =============================================================
    // SUPPLIER OUTSTANDING
    // =============================================================

    public class SupplierOutstandingViewModel
    {
        public string Search { get; set; } = string.Empty;

        public decimal TotalOutstanding { get; set; }

        public List<SupplierOutstandingRowViewModel>
            Items
        { get; set; } = new();
    }


    public class SupplierOutstandingRowViewModel
    {
        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public string? SupplierCode { get; set; }

        public string? Phone { get; set; }

        public decimal GoodsReceivedValue { get; set; }

        public decimal PaymentsMade { get; set; }

        public decimal OutstandingPayable { get; set; }
    }


    // =============================================================
    // SUPPLIER LEDGER
    // =============================================================

    public class SupplierLedgerViewModel
    {
        public int? SupplierId { get; set; }

        public string SupplierName { get; set; }
            = string.Empty;

        // =========================================================
        // DATE FILTER
        // =========================================================

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        // =========================================================
        // OPENING BALANCE
        // =========================================================

        public decimal OpeningBalance { get; set; }

        // =========================================================
        // SELECTED PERIOD TOTALS
        // =========================================================

        public decimal GoodsReceivedValue { get; set; }

        public decimal PaymentsMade { get; set; }

        // =========================================================
        // CLOSING BALANCE
        // =========================================================

        public decimal ClosingOutstanding { get; set; }

        // =========================================================
        // LEDGER ENTRIES
        // =========================================================

        public List<SupplierLedgerRowViewModel> Entries { get; set; }
            = new List<SupplierLedgerRowViewModel>();
    }


    public class SupplierLedgerRowViewModel
    {
        public DateTime Date { get; set; }

        public string ReferenceNo { get; set; }
            = string.Empty;

        public string TransactionType { get; set; }
            = string.Empty;

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal Balance { get; set; }

        public string? Remarks { get; set; }
    }


// =============================================================
// SUPPLIER PAYMENTS
// =============================================================

public class SupplierPaymentsViewModel
    {
        public string Search { get; set; } = string.Empty;

        public int? SupplierId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal PaymentsMade { get; set; }

        public List<SupplierPaymentRowViewModel>
            Payments
        { get; set; } = new();
    }


    public class SupplierPaymentRowViewModel
    {
        public int Id { get; set; }

        public string PaymentNumber { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        public string? ReferenceNumber { get; set; }

        public string? BankName { get; set; }

        public string? Remarks { get; set; }

        public bool IsCancelled { get; set; }

        public string? CancellationRemarks { get; set; }
    }


    // =============================================================
    // CREATE SUPPLIER PAYMENT
    // =============================================================

    public class SupplierPaymentCreateViewModel
    {
        [Required(ErrorMessage = "Supplier is required.")]
        public int SupplierId { get; set; }


        [Required(ErrorMessage = "Payment date is required.")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;


        [Required(ErrorMessage = "Payment amount is required.")]
        [Range(
            typeof(decimal),
            "0.01",
            "999999999999",
            ErrorMessage = "Enter a valid payment amount.")]
        public decimal Amount { get; set; }


        [Required(ErrorMessage = "Payment mode is required.")]
        [StringLength(30)]
        public string PaymentMode { get; set; } = "Bank";


        [StringLength(100)]
        public string? ReferenceNumber { get; set; }


        [StringLength(100)]
        public string? BankName { get; set; }


        [StringLength(500)]
        public string? Remarks { get; set; }


        // =========================================================
        // CURRENT SUPPLIER OUTSTANDING
        // =========================================================

        /// <summary>
        /// Current outstanding payable amount for selected supplier.
        /// </summary>
        public decimal CurrentOutstanding { get; set; }


        // =========================================================
        // SUPPLIER DROPDOWN
        // =========================================================

        public List<SupplierSelectItemViewModel>
            Suppliers
        { get; set; } = new();
    }


    public class SupplierSelectItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Current outstanding payable amount for this supplier.
        /// </summary>
        public decimal Outstanding { get; set; }
    }


    // =============================================================
    // CANCEL PAYMENT
    // =============================================================

    public class SupplierPaymentCancelViewModel
    {
        public int Id { get; set; }

        public string PaymentNumber { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public decimal Amount { get; set; }


        [Required(
            ErrorMessage =
                "Cancellation reason is required.")]
        [StringLength(500)]
        public string CancellationRemarks { get; set; }
            = string.Empty;
    }
}