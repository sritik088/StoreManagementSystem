namespace StoreManagementSystem.Web.ViewModels.ProductStatement
{
    public class ProductStatementViewModel
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<ProductStatementRowViewModel> Rows { get; set; }
            = new List<ProductStatementRowViewModel>();
    }

    public class ProductStatementRowViewModel
    {
        public DateTime Date { get; set; }

        public string GRNNumber { get; set; } = "-";

        public string Subcategory { get; set; } = "-";

        // ============================================================
        // PURCHASE
        // ============================================================

        public decimal PurchaseQuantity { get; set; }

        public decimal PurchaseRate { get; set; }

        // ============================================================
        // SALE
        // ============================================================

        public decimal SaleQuantity { get; set; }

        public decimal SaleRate { get; set; }

        // ============================================================
        // DAMAGE
        // ============================================================

        public decimal DamageQuantity { get; set; }

        public decimal DamageRate { get; set; }

        // ============================================================
        // BALANCE
        // ============================================================

        public decimal BalanceQuantity { get; set; }

        public decimal BalanceRate { get; set; }
    }
}