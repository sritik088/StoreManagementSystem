using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class SupplierPayment
    {
        public int Id { get; set; }


    // =========================================================
    // PAYMENT NUMBER
    // =========================================================

    [Required]
        [StringLength(30)]
        public string PaymentNumber { get; set; } = string.Empty;

        // =========================================================
        // SUPPLIER
        // =========================================================

        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        // =========================================================
        // PAYMENT DATE
        // =========================================================

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // =========================================================
        // AMOUNT
        // =========================================================

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        // =========================================================
        // PAYMENT MODE
        // Cash / Bank / UPI / Cheque / NEFT / RTGS / IMPS
        // =========================================================

        [Required]
        [StringLength(30)]
        public string PaymentMode { get; set; } = "Bank";

        // =========================================================
        // REFERENCE
        // =========================================================

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        // =========================================================
        // REMARKS
        // =========================================================

        [StringLength(500)]
        public string? Remarks { get; set; }

        // =========================================================
        // CANCELLATION / REVERSAL
        // =========================================================

        public bool IsCancelled { get; set; } = false;

        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        public string? CancellationRemarks { get; set; }

        // =========================================================
        // AUDIT
        // =========================================================

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }

}
