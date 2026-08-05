namespace StoreManagementSystem.Domain.Entities;

public class StockTransferItem
{
    public int Id { get; set; }

    public int StockTransferId { get; set; }

    public StockTransfer? StockTransfer { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }
}