using StoreManagementSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities;

public class StockTransfer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string TransferNumber { get; set; } = string.Empty;

    public DateTime TransferDate { get; set; } = DateTime.Now;

    public int FromWarehouseId { get; set; }

    public Warehouse? FromWarehouse { get; set; }

    public int ToWarehouseId { get; set; }

    public Warehouse? ToWarehouse { get; set; }

    public StockTransferStatus Status { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public ICollection<StockTransferItem> Items { get; set; }
        = new List<StockTransferItem>();

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}