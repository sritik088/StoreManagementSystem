
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class GoodsReceiptService : IGoodsReceiptService
    {
        private readonly IGoodsReceiptRepository _repository;
        private readonly IStockLedgerRepository _stockLedgerRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IStockLedgerService _stockLedgerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWarehouseStockService _warehouseStockService;

        public GoodsReceiptService(
            IGoodsReceiptRepository repository,
            IStockLedgerRepository stockLedgerRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IStockLedgerService stockLedgerService,
            IUnitOfWork unitOfWork,
            IWarehouseStockService warehouseStockService)
        {
            _repository = repository;
            _stockLedgerRepository = stockLedgerRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _stockLedgerService = stockLedgerService;
            _unitOfWork = unitOfWork;
            _warehouseStockService = warehouseStockService;
        }

        // =====================================================
        // VALIDATION HELPERS
        // =====================================================

        private static void ValidateItems(
            GoodsReceipt receipt,
            bool isGift)
        {
            if (receipt.Items == null || receipt.Items.Count == 0)
                throw new InvalidOperationException(
                    "At least one receipt item is required.");

            foreach (var item in receipt.Items)
            {
                if (item.ProductId <= 0)
                    throw new InvalidOperationException(
                        "Please select a valid product.");

                if (item.WarehouseId <= 0)
                    throw new InvalidOperationException(
                        "Please select a valid warehouse.");

                if (item.ReceivedQuantity <= 0)
                    throw new InvalidOperationException(
                        "Received quantity must be greater than zero.");

                if (item.Discount < 0 || item.TaxAmount < 0)
                    throw new InvalidOperationException(
                        "Discount and tax cannot be negative.");

                if (isGift)
                {
                    item.PurchaseOrderItemId = null;
                    item.OrderedQuantity = 0m;
                    item.UnitPrice = 0m;
                    item.Discount = 0m;
                    item.TaxAmount = 0m;
                    item.Total = 0m;
                }
            }
        }

        private static void PrepareReceipt(
            GoodsReceipt receipt,
            GoodsReceiptType receiptType)
        {
            receipt.GRNNumber = string.Empty;
            receipt.ReceiptType = receiptType;
            receipt.ReceiptDate = receipt.ReceiptDate == default
                ? DateTime.Now
                : receipt.ReceiptDate;

            receipt.Status = GoodsReceiptStatus.Received;
            receipt.CreatedDate = DateTime.Now;
            receipt.UpdatedDate = null;
            receipt.IsDeleted = false;

            receipt.SubTotal = receipt.Items.Sum(
                x => x.ReceivedQuantity * x.UnitPrice);

            receipt.Discount = receipt.Items.Sum(x => x.Discount);
            receipt.TaxAmount = receipt.Items.Sum(x => x.TaxAmount);

            receipt.GrandTotal =
                receipt.SubTotal -
                receipt.Discount +
                receipt.TaxAmount;
        }

        private async Task AddStockAndLedgerAsync(
            GoodsReceipt receipt,
            StockTransactionType transactionType,
            string remarks,
            bool reverse = false)
        {
            foreach (var item in receipt.Items)
            {
                decimal quantity = reverse
                    ? -item.ReceivedQuantity
                    : item.ReceivedQuantity;

                if (reverse)
                {
                    await _warehouseStockService.DecreaseStockAsync(
                        item.WarehouseId,
                        item.ProductId,
                        item.ReceivedQuantity);
                }
                else
                {
                    await _warehouseStockService.IncreaseStockAsync(
                        item.WarehouseId,
                        item.ProductId,
                        item.ReceivedQuantity);
                }

                await _stockLedgerService.AddStockMovementAsync(
                    productId: item.ProductId,
                    warehouseId: item.WarehouseId,
                    quantity: quantity,
                    transactionType: transactionType,
                    unitCost: item.UnitPrice,
                    referenceNo: reverse
                        ? $"REV-{receipt.GRNNumber}"
                        : receipt.GRNNumber,
                    remarks: remarks);
            }
        }

        // =====================================================
        // CREATE PURCHASE GRN
        // =====================================================

        public async Task<bool> CreateAsync(GoodsReceipt receipt)
        {
            ArgumentNullException.ThrowIfNull(receipt);

            ValidateItems(receipt, isGift: false);

            if (!receipt.PurchaseOrderId.HasValue ||
                receipt.PurchaseOrderId.Value <= 0)
            {
                throw new InvalidOperationException(
                    "Purchase Order is required.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var purchaseOrder =
                    await _repository.GetPurchaseOrderWithDetailsAsync(
                        receipt.PurchaseOrderId.Value);

                if (purchaseOrder == null)
                    throw new InvalidOperationException(
                        "Purchase Order not found or deleted.");

                if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
                    purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
                {
                    throw new InvalidOperationException(
                        "Only Approved or Partially Received Purchase Orders can receive goods.");
                }

                // Prevent duplicate PO-item lines from bypassing
                // the remaining-quantity validation.
                var requestedByPoItem = receipt.Items
                    .GroupBy(x => x.PurchaseOrderItemId)
                    .ToList();

                if (requestedByPoItem.Any(x => !x.Key.HasValue || x.Key.Value <= 0))
                {
                    throw new InvalidOperationException(
                        "Purchase Order Item is required for every receipt item.");
                }

                foreach (var group in requestedByPoItem)
                {
                    var poItem = purchaseOrder.Items.FirstOrDefault(
                        x => x.Id == group.Key!.Value);

                    if (poItem == null)
                        throw new InvalidOperationException(
                            "Invalid Purchase Order Item.");

                    if (group.Any(x => x.ProductId != poItem.ProductId))
                        throw new InvalidOperationException(
                            "Selected product does not match the Purchase Order Item.");

                    decimal previouslyReceived =
                        await _repository.GetPreviouslyReceivedQuantityAsync(
                            poItem.Id);

                    decimal remaining = poItem.Quantity - previouslyReceived;
                    decimal requested = group.Sum(x => x.ReceivedQuantity);

                    if (remaining <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Product {poItem.Product?.SKU ?? poItem.ProductId.ToString()} is already fully received.");
                    }

                    if (requested > remaining)
                    {
                        throw new InvalidOperationException(
                            $"Cannot receive {requested:N2} units. " +
                            $"Only {remaining:N2} units remain for this Purchase Order Item.");
                    }

                    foreach (var item in group)
                    {
                        item.OrderedQuantity = poItem.Quantity;
                        item.UnitPrice = poItem.UnitPrice;

                        decimal gross = item.ReceivedQuantity * item.UnitPrice;

                        if (item.Discount > gross)
                        {
                            throw new InvalidOperationException(
                                "An item discount cannot exceed its gross amount.");
                        }

                        item.Total = gross - item.Discount + item.TaxAmount;
                    }
                }

                // Use the supplier saved on the Purchase Order.
                receipt.SupplierId = purchaseOrder.SupplierId;
                receipt.ReceiptType = GoodsReceiptType.Purchase;

                PrepareReceipt(receipt, GoodsReceiptType.Purchase);
                receipt.GRNNumber = await _repository.GenerateGRNNumberAsync();

                await _repository.AddAsync(receipt);
                await _unitOfWork.SaveChangesAsync();

                await AddStockAndLedgerAsync(
                    receipt,
                    StockTransactionType.PurchaseReceipt,
                    $"Goods received against PO {purchaseOrder.PONumber}");

                await _unitOfWork.SaveChangesAsync();

                await _repository.RecalculatePurchaseOrderStatusAsync(
                    purchaseOrder.Id);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // =====================================================
        // CREATE GIFT GRN
        // =====================================================

        public async Task<bool> CreateGiftAsync(GoodsReceipt receipt)
        {
            ArgumentNullException.ThrowIfNull(receipt);

            if (string.IsNullOrWhiteSpace(receipt.DonorName))
                throw new InvalidOperationException(
                    "Donor name is required for Gift GRN.");

            ValidateItems(receipt, isGift: true);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                receipt.PurchaseOrderId = null;
                receipt.SupplierId = null;
                receipt.GiftReason = receipt.GiftReason?.Trim();
                receipt.DonorName = receipt.DonorName.Trim();

                PrepareReceipt(receipt, GoodsReceiptType.Gift);

                // Gifts have no purchase value in this system.
                receipt.SubTotal = 0m;
                receipt.Discount = 0m;
                receipt.TaxAmount = 0m;
                receipt.GrandTotal = 0m;

                receipt.GRNNumber = await _repository.GenerateGRNNumberAsync();

                await _repository.AddAsync(receipt);
                await _unitOfWork.SaveChangesAsync();

                await AddStockAndLedgerAsync(
                    receipt,
                    StockTransactionType.GiftReceipt,
                    $"Gift received from {receipt.DonorName}");

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // =====================================================
        // GET METHODS
        // =====================================================

        public async Task<IEnumerable<GoodsReceipt>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<GoodsReceipt?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<decimal> GetPreviouslyReceivedQuantityAsync(
            int purchaseOrderItemId)
        {
            return await _repository.GetPreviouslyReceivedQuantityAsync(
                purchaseOrderItemId);
        }

        // =====================================================
        // DELETE GRN: REVERSE STOCK AND SOFT DELETE
        // =====================================================

        public async Task<bool> DeleteAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var receipt = await _repository.GetByIdAsync(id);

                if (receipt == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return false;
                }

                if (receipt.Items == null || receipt.Items.Count == 0)
                    throw new InvalidOperationException(
                        $"GRN {receipt.GRNNumber} does not contain any items.");

                var reversalItems = receipt.Items
                    .GroupBy(x => new { x.ProductId, x.WarehouseId })
                    .Select(g => new
                    {
                        g.Key.ProductId,
                        g.Key.WarehouseId,
                        Quantity = g.Sum(x => x.ReceivedQuantity)
                    })
                    .ToList();

                // Validate all stock before making any changes.
                foreach (var item in reversalItems)
                {
                    decimal available =
                        await _warehouseStockService.GetAvailableStockAsync(
                            item.WarehouseId,
                            item.ProductId);

                    if (available < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Cannot delete GRN {receipt.GRNNumber}. " +
                            $"Stock reversal requires {item.Quantity:N2} units, " +
                            $"but only {available:N2} units are available for " +
                            $"Product ID {item.ProductId}.");
                    }
                }

                StockTransactionType reversalType =
                    receipt.ReceiptType == GoodsReceiptType.Gift
                        ? StockTransactionType.GiftReceiptReversal
                        : StockTransactionType.PurchaseReceiptReversal;

                // Reverse stock and write reversal ledger entries.
                foreach (var item in reversalItems)
                {
                    await _warehouseStockService.DecreaseStockAsync(
                        item.WarehouseId,
                        item.ProductId,
                        item.Quantity);

                    await _stockLedgerService.AddStockMovementAsync(
                        productId: item.ProductId,
                        warehouseId: item.WarehouseId,
                        quantity: -item.Quantity,
                        transactionType: reversalType,
                        unitCost: receipt.Items
                            .First(x => x.ProductId == item.ProductId &&
                                        x.WarehouseId == item.WarehouseId)
                            .UnitPrice,
                        referenceNo: $"REV-{receipt.GRNNumber}",
                        remarks: $"Stock reversal for deleted {receipt.ReceiptType} GRN {receipt.GRNNumber}");
                }

                // Save the soft deletion before recalculating PO totals.
                // The repository recalculation queries the database.
                await _repository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                if (receipt.ReceiptType == GoodsReceiptType.Purchase &&
                    receipt.PurchaseOrderId.HasValue)
                {
                    await _repository.RecalculatePurchaseOrderStatusAsync(
                        receipt.PurchaseOrderId.Value);

                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // =====================================================
        // RESTORE GRN: RE-APPLY STOCK AND UNDELETE
        // =====================================================

        public async Task<bool> RestoreAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var receipt = await _repository.GetDeletedByIdAsync(id);

                if (receipt == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return false;
                }

                if (receipt.Items == null || receipt.Items.Count == 0)
                    throw new InvalidOperationException(
                        $"GRN {receipt.GRNNumber} does not contain any items.");

                if (receipt.ReceiptType == GoodsReceiptType.Purchase)
                {
                    if (!receipt.PurchaseOrderId.HasValue)
                        throw new InvalidOperationException(
                            "The Purchase Order linked to this GRN is missing.");

                    var purchaseOrder =
                        await _purchaseOrderRepository.GetByIdWithDetailsAsync(
                            receipt.PurchaseOrderId.Value);

                    if (purchaseOrder == null || purchaseOrder.IsDeleted)
                        throw new InvalidOperationException(
                            "Restore the related Purchase Order before restoring this GRN.");

                    // Check cumulative received quantities, excluding this
                    // deleted GRN, before restoring its quantities.
                    foreach (var group in receipt.Items
                        .Where(x => x.PurchaseOrderItemId.HasValue)
                        .GroupBy(x => x.PurchaseOrderItemId!.Value))
                    {
                        var poItem = purchaseOrder.Items.FirstOrDefault(
                            x => x.Id == group.Key);

                        if (poItem == null)
                            throw new InvalidOperationException(
                                "A Purchase Order Item linked to this GRN no longer exists.");

                        decimal activeReceived =
                            await _repository.GetPreviouslyReceivedQuantityAsync(
                                poItem.Id);

                        decimal restoring = group.Sum(
                            x => x.ReceivedQuantity);

                        if (activeReceived + restoring > poItem.Quantity)
                        {
                            throw new InvalidOperationException(
                                $"Cannot restore GRN {receipt.GRNNumber}. " +
                                $"Restoring this GRN would exceed the ordered quantity " +
                                $"for Product ID {poItem.ProductId}.");
                        }
                    }
                }

                var restoreItems = receipt.Items
                    .GroupBy(x => new { x.ProductId, x.WarehouseId })
                    .Select(g => new
                    {
                        g.Key.ProductId,
                        g.Key.WarehouseId,
                        Quantity = g.Sum(x => x.ReceivedQuantity),
                        UnitCost = g.First().UnitPrice
                    })
                    .ToList();

                StockTransactionType restoreType =
                    receipt.ReceiptType == GoodsReceiptType.Gift
                        ? StockTransactionType.GiftReceipt
                        : StockTransactionType.PurchaseReceipt;

                foreach (var item in restoreItems)
                {
                    await _warehouseStockService.IncreaseStockAsync(
                        item.WarehouseId,
                        item.ProductId,
                        item.Quantity);

                    await _stockLedgerService.AddStockMovementAsync(
                        productId: item.ProductId,
                        warehouseId: item.WarehouseId,
                        quantity: item.Quantity,
                        transactionType: restoreType,
                        unitCost: item.UnitCost,
                        referenceNo: $"RESTORE-{receipt.GRNNumber}",
                        remarks: $"Stock restored for GRN {receipt.GRNNumber}");
                }

                // Persist restoration before recalculating PO totals so the
                // repository query sees this GRN as active.
                await _repository.RestoreAsync(id);
                await _unitOfWork.SaveChangesAsync();

                if (receipt.ReceiptType == GoodsReceiptType.Purchase &&
                    receipt.PurchaseOrderId.HasValue)
                {
                    await _repository.RecalculatePurchaseOrderStatusAsync(
                        receipt.PurchaseOrderId.Value);

                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
