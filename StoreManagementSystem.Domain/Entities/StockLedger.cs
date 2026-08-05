using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Domain.Entities
{
    public class StockLedger
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        public StockTransactionType TransactionType { get; set; }

        public decimal Quantity { get; set; }

        public decimal BalanceAfterTransaction { get; set; }

        public decimal UnitCost { get; set; }

        public string ReferenceNo { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; }

        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
    }
}