using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class GoodsReceiptService : IGoodsReceiptService
    {
        private readonly IGoodsReceiptRepository
            _repository;

        private readonly IStockLedgerRepository
            _stockLedgerRepository;

        private readonly IPurchaseOrderRepository
            _purchaseOrderRepository;

        private readonly IStockLedgerService
            _stockLedgerService;

        private readonly IUnitOfWork
            _unitOfWork;

        private readonly IWarehouseStockService
            _warehouseStockService;


        public GoodsReceiptService(
            IGoodsReceiptRepository repository,
            IStockLedgerRepository stockLedgerRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IStockLedgerService stockLedgerService,
            IUnitOfWork unitOfWork,
            IWarehouseStockService warehouseStockService)
        {
            _repository =
                repository;

            _stockLedgerRepository =
                stockLedgerRepository;

            _purchaseOrderRepository =
                purchaseOrderRepository;

            _stockLedgerService =
                stockLedgerService;

            _unitOfWork =
                unitOfWork;

            _warehouseStockService =
                warehouseStockService;
        }


        // =====================================================
        // CREATE PURCHASE GRN
        // =====================================================

        public async Task<bool>
            CreateAsync(
                GoodsReceipt receipt)
        {
            if (receipt == null)
                throw new ArgumentNullException(
                    nameof(receipt));

            if (receipt.Items == null ||
                receipt.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one item is required.");
            }

            if (!receipt.PurchaseOrderId.HasValue ||
                receipt.PurchaseOrderId.Value <= 0)
            {
                throw new InvalidOperationException(
                    "Purchase Order is required.");
            }

            await _unitOfWork
                .BeginTransactionAsync();

            try
            {
                // =================================================
                // 1. LOAD PURCHASE ORDER
                // =================================================

                var purchaseOrder =
                    await _repository
                        .GetPurchaseOrderWithDetailsAsync(
                            receipt.PurchaseOrderId.Value);

                if (purchaseOrder == null)
                {
                    throw new InvalidOperationException(
                        "Purchase Order not found.");
                }

                if (purchaseOrder.Status !=
                        PurchaseOrderStatus.Approved &&
                    purchaseOrder.Status !=
                        PurchaseOrderStatus.PartiallyReceived)
                {
                    throw new InvalidOperationException(
                        "Only Approved or Partially Received Purchase Orders can receive goods.");
                }


                // =================================================
                // 2. VALIDATE ITEMS
                // =================================================

                foreach (var item in receipt.Items)
                {
                    if (item.ProductId <= 0)
                    {
                        throw new InvalidOperationException(
                            "Please select a product.");
                    }

                    if (item.WarehouseId <= 0)
                    {
                        throw new InvalidOperationException(
                            "Please select a warehouse.");
                    }

                    if (item.ReceivedQuantity <= 0)
                    {
                        throw new InvalidOperationException(
                            "Received quantity must be greater than zero.");
                    }

                    if (item.PurchaseOrderItemId <= 0)
                    {
                        throw new InvalidOperationException(
                            "Purchase Order Item is required.");
                    }


                    var poItem =
                        purchaseOrder.Items
                            .FirstOrDefault(x =>
                                x.Id ==
                                item.PurchaseOrderItemId.Value);

                    if (poItem == null)
                    {
                        throw new InvalidOperationException(
                            "Invalid Purchase Order Item.");
                    }


                    if (poItem.ProductId !=
                        item.ProductId)
                    {
                        throw new InvalidOperationException(
                            "Selected product does not match the Purchase Order Item.");
                    }


                    // =================================================
                    // PREVIOUSLY RECEIVED
                    // =================================================

                    decimal previouslyReceived =
                        await _repository
                            .GetPreviouslyReceivedQuantityAsync(
                                poItem.Id);


                    decimal remainingQuantity =
                        poItem.Quantity -
                        previouslyReceived;


                    if (remainingQuantity <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Product {poItem.Product?.SKU ?? poItem.ProductId.ToString()} is already fully received.");
                    }


                    if (item.ReceivedQuantity >
                        remainingQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Cannot receive {item.ReceivedQuantity:N2} units. " +
                            $"Only {remainingQuantity:N2} units remain for this Purchase Order Item.");
                    }


                    // =================================================
                    // TRUST DATABASE PO VALUES
                    // =================================================

                    item.OrderedQuantity =
                        poItem.Quantity;

                    item.UnitPrice =
                        poItem.UnitPrice;


                    item.Total =
                        (
                            item.ReceivedQuantity *
                            item.UnitPrice
                        )
                        - item.Discount
                        + item.TaxAmount;
                }


                // =================================================
                // 3. PREPARE GRN
                // =================================================

                receipt.GRNNumber =
                    await _repository
                        .GenerateGRNNumberAsync();

                receipt.ReceiptType =
                    GoodsReceiptType.Purchase;

                receipt.ReceiptDate =
                    receipt.ReceiptDate == default
                        ? DateTime.Now
                        : receipt.ReceiptDate;

                receipt.Status =
                    GoodsReceiptStatus.Received;

                receipt.CreatedDate =
                    DateTime.Now;

                receipt.UpdatedDate =
                    null;

                receipt.IsDeleted =
                    false;


                // =================================================
                // 4. CALCULATE TOTALS
                // =================================================

                receipt.SubTotal =
                    receipt.Items.Sum(x =>
                        x.ReceivedQuantity *
                        x.UnitPrice);

                receipt.Discount =
                    receipt.Items.Sum(x =>
                        x.Discount);

                receipt.TaxAmount =
                    receipt.Items.Sum(x =>
                        x.TaxAmount);

                receipt.GrandTotal =
                    receipt.SubTotal
                    - receipt.Discount
                    + receipt.TaxAmount;


                // =================================================
                // 5. SAVE GRN
                // =================================================

                await _repository
                    .AddAsync(receipt);

                await _unitOfWork
                    .SaveChangesAsync();


                // =================================================
                // 6. INCREASE WAREHOUSE STOCK
                // =================================================

                foreach (var item in receipt.Items)
                {
                    await _warehouseStockService
                        .IncreaseStockAsync(
                            item.WarehouseId,
                            item.ProductId,
                            item.ReceivedQuantity);
                }


                // =================================================
                // 7. STOCK LEDGER
                // =================================================

                foreach (var item in receipt.Items)
                {
                    await _stockLedgerService
                        .AddStockMovementAsync(
                            productId:
                                item.ProductId,

                            warehouseId:
                                item.WarehouseId,

                            quantity:
                                item.ReceivedQuantity,

                            transactionType:
                                StockTransactionType.PurchaseReceipt,

                            unitCost:
                                item.UnitPrice,

                            referenceNo:
                                receipt.GRNNumber,

                            remarks:
                                $"Goods received against PO {purchaseOrder.PONumber}");
                }


                // =================================================
                // 8. SAVE STOCK
                // =================================================

                await _unitOfWork
                    .SaveChangesAsync();


                // =================================================
                // 9. RECALCULATE PO STATUS
                // =================================================

                await _repository
                    .RecalculatePurchaseOrderStatusAsync(
                        purchaseOrder.Id);


                // =================================================
                // 10. COMMIT
                // =================================================

                await _unitOfWork
                    .CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork
                    .RollbackAsync();

                throw;
            }
        }


        // =====================================================
        // CREATE GIFT GRN
        // =====================================================

        public async Task<bool>
            CreateGiftAsync(
                GoodsReceipt receipt)
        {
            if (receipt == null)
                throw new ArgumentNullException(
                    nameof(receipt));

            if (receipt.Items == null ||
                receipt.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one gift item is required.");
            }


            receipt.ReceiptType =
                GoodsReceiptType.Gift;

            receipt.PurchaseOrderId =
                null;

            receipt.SupplierId =
                null;


            if (string.IsNullOrWhiteSpace(
                receipt.DonorName))
            {
                throw new InvalidOperationException(
                    "Donor name is required for Gift GRN.");
            }


            await _unitOfWork
                .BeginTransactionAsync();

            try
            {
                // =================================================
                // 1. VALIDATE ITEMS
                // =================================================

                foreach (var item in receipt.Items)
                {
                    if (item.ProductId <= 0)
                    {
                        throw new InvalidOperationException(
                            "Please select a product.");
                    }

                    if (item.WarehouseId <= 0)
                    {
                        throw new InvalidOperationException(
                            "Please select a warehouse.");
                    }

                    if (item.ReceivedQuantity <= 0)
                    {
                        throw new InvalidOperationException(
                            "Gift quantity must be greater than zero.");
                    }


                    item.PurchaseOrderItemId =
                        null;

                    item.OrderedQuantity =
                        0m;

                    item.UnitPrice =
                        0m;

                    item.Discount =
                        0m;

                    item.TaxAmount =
                        0m;

                    item.Total =
                        0m;
                }


                // =================================================
                // 2. PREPARE GIFT GRN
                // =================================================

                receipt.GRNNumber =
                    await _repository
                        .GenerateGRNNumberAsync();

                receipt.ReceiptType =
                    GoodsReceiptType.Gift;

                receipt.PurchaseOrderId =
                    null;

                receipt.SupplierId =
                    null;

                receipt.ReceiptDate =
                    receipt.ReceiptDate == default
                        ? DateTime.Now
                        : receipt.ReceiptDate;

                receipt.Status =
                    GoodsReceiptStatus.Received;

                receipt.SubTotal =
                    0m;

                receipt.Discount =
                    0m;

                receipt.TaxAmount =
                    0m;

                receipt.GrandTotal =
                    0m;

                receipt.CreatedDate =
                    DateTime.Now;

                receipt.UpdatedDate =
                    null;

                receipt.IsDeleted =
                    false;


                // =================================================
                // 3. SAVE GIFT GRN
                // =================================================

                await _repository
                    .AddAsync(receipt);

                await _unitOfWork
                    .SaveChangesAsync();


                // =================================================
                // 4. INCREASE STOCK
                // =================================================

                foreach (var item in receipt.Items)
                {
                    await _warehouseStockService
                        .IncreaseStockAsync(
                            item.WarehouseId,
                            item.ProductId,
                            item.ReceivedQuantity);
                }


                // =================================================
                // 5. GIFT STOCK LEDGER
                // =================================================

                foreach (var item in receipt.Items)
                {
                    await _stockLedgerService
                        .AddStockMovementAsync(
                            productId:
                                item.ProductId,

                            warehouseId:
                                item.WarehouseId,

                            quantity:
                                item.ReceivedQuantity,

                            transactionType:
                                StockTransactionType.GiftReceipt,

                            unitCost:
                                0m,

                            referenceNo:
                                receipt.GRNNumber,

                            remarks:
                                $"Gift received from {receipt.DonorName}");
                }


                // =================================================
                // 6. SAVE
                // =================================================

                await _unitOfWork
                    .SaveChangesAsync();


                // =================================================
                // 7. COMMIT
                // =================================================

                await _unitOfWork
                    .CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork
                    .RollbackAsync();

                throw;
            }
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<GoodsReceipt>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }


        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<GoodsReceipt?>
            GetByIdAsync(int id)
        {
            return await _repository
                .GetByIdAsync(id);
        }


        // =====================================================
        // PREVIOUSLY RECEIVED
        // =====================================================

        public async Task<decimal>
            GetPreviouslyReceivedQuantityAsync(
                int purchaseOrderItemId)
        {
            return await _repository
                .GetPreviouslyReceivedQuantityAsync(
                    purchaseOrderItemId);
        }


        // =====================================================
        // DELETE = REVERSE + SOFT DELETE
        // =====================================================

        public async Task<bool>
            DeleteAsync(int id)
        {
            await _unitOfWork
                .BeginTransactionAsync();

            try
            {
                // =================================================
                // 1. LOAD ACTIVE GRN
                // =================================================

                var receipt =
                    await _repository
                        .GetByIdAsync(id);

                if (receipt == null)
                {
                    await _unitOfWork
                        .RollbackAsync();

                    return false;
                }


                if (receipt.Items == null ||
                    receipt.Items.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"GRN {receipt.GRNNumber} does not contain any items.");
                }


                // =================================================
                // 2. GROUP ITEMS
                // =================================================

                var reversalItems =
                    receipt.Items
                        .GroupBy(x => new
                        {
                            x.ProductId,
                            x.WarehouseId
                        })
                        .Select(g => new
                        {
                            g.Key.ProductId,
                            g.Key.WarehouseId,

                            Quantity =
                                g.Sum(x =>
                                    x.ReceivedQuantity),

                            UnitCost =
                                g.First().UnitPrice
                        })
                        .ToList();


                // =================================================
                // 3. VALIDATE ALL STOCK FIRST
                // =================================================

                foreach (var item in reversalItems)
                {
                    decimal available =
                        await _warehouseStockService
                            .GetAvailableStockAsync(
                                item.WarehouseId,
                                item.ProductId);

                    if (available < item.Quantity)
                    {
                        var productName =
                            receipt.Items
                                .Where(x =>
                                    x.ProductId ==
                                    item.ProductId)
                                .Select(x =>
                                    x.Product?.SKU)
                                .FirstOrDefault();

                        if (string.IsNullOrWhiteSpace(
                            productName))
                        {
                            productName =
                                $"Product ID {item.ProductId}";
                        }


                        throw new InvalidOperationException(
                            $"Cannot delete GRN {receipt.GRNNumber}. " +
                            $"Stock reversal requires {item.Quantity:N2} units of " +
                            $"{productName}, but only {available:N2} units are currently available. " +
                            $"The received stock may already have been sold, transferred, damaged, issued, or reserved.");
                    }
                }


                // =================================================
                // 4. REVERSE WAREHOUSE STOCK
                // =================================================

                foreach (var item in reversalItems)
                {
                    await _warehouseStockService
                        .DecreaseStockAsync(
                            item.WarehouseId,
                            item.ProductId,
                            item.Quantity);
                }


                // =================================================
                // 5. CREATE REVERSAL LEDGER
                // =================================================

                StockTransactionType reversalType =
                    receipt.ReceiptType ==
                        GoodsReceiptType.Gift

                        ? StockTransactionType
                            .GiftReceiptReversal

                        : StockTransactionType
                            .PurchaseReceiptReversal;


                string reversalReference =
                    $"REV-{receipt.GRNNumber}";


                foreach (var item in reversalItems)
                {
                    await _stockLedgerService
                        .AddStockMovementAsync(
                            productId:
                                item.ProductId,

                            warehouseId:
                                item.WarehouseId,

                            quantity:
                                -item.Quantity,

                            transactionType:
                                reversalType,

                            unitCost:
                                item.UnitCost,

                            referenceNo:
                                reversalReference,

                            remarks:
                                $"Stock reversal for deleted {receipt.ReceiptType} GRN {receipt.GRNNumber}");
                }


                // =================================================
                // 6. RECALCULATE PO STATUS
                // =================================================

                if (receipt.ReceiptType ==
                        GoodsReceiptType.Purchase &&
                    receipt.PurchaseOrderId.HasValue)
                {
                    await _repository
                        .RecalculatePurchaseOrderStatusAsync(
                            receipt.PurchaseOrderId.Value);
                }


                // =================================================
                // 7. SOFT DELETE GRN
                // =================================================

                await _repository
                    .DeleteAsync(id);


                // =================================================
                // 8. SAVE EVERYTHING
                // =================================================

                await _unitOfWork
                    .SaveChangesAsync();


                // =================================================
                // 9. COMMIT
                // =================================================

                await _unitOfWork
                    .CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork
                    .RollbackAsync();

                throw;
            }
        }


        // =====================================================
        // RESTORE = RE-APPLY STOCK + LEDGER + UNDELETE
        // =====================================================

        public async Task<bool>
            RestoreAsync(int id)
        {
            await _unitOfWork
                .BeginTransactionAsync();

            try
            {
                // =================================================
                // 1. LOAD DELETED GRN
                // =================================================

                var receipt =
                    await _repository
                        .GetDeletedByIdAsync(id);

                if (receipt == null)
                {
                    await _unitOfWork
                        .RollbackAsync();

                    return false;
                }


                if (!receipt.IsDeleted)
                {
                    await _unitOfWork
                        .RollbackAsync();

                    return false;
                }


                if (receipt.Items == null ||
                    receipt.Items.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"GRN {receipt.GRNNumber} does not contain any items.");
                }


                // =================================================
                // 2. GROUP ITEMS
                // =================================================

                var restoreItems =
                    receipt.Items
                        .GroupBy(x => new
                        {
                            x.ProductId,
                            x.WarehouseId
                        })
                        .Select(g => new
                        {
                            g.Key.ProductId,
                            g.Key.WarehouseId,

                            Quantity =
                                g.Sum(x =>
                                    x.ReceivedQuantity),

                            UnitCost =
                                g.First().UnitPrice
                        })
                        .ToList();


                // =================================================
                // 3. VALIDATE PURCHASE ORDER
                // =================================================

                if (receipt.ReceiptType ==
                        GoodsReceiptType.Purchase &&
                    receipt.PurchaseOrderId.HasValue)
                {
                    var purchaseOrder =
                        await _purchaseOrderRepository
                            .GetByIdWithDetailsAsync(
                                receipt.PurchaseOrderId.Value);

                    if (purchaseOrder == null)
                    {
                        throw new InvalidOperationException(
                            $"Purchase Order for GRN {receipt.GRNNumber} was not found.");
                    }

                    if (purchaseOrder.IsDeleted)
                    {
                        throw new InvalidOperationException(
                            $"Purchase Order {purchaseOrder.PONumber} is deleted. " +
                            $"Restore the Purchase Order before restoring this GRN.");
                    }
                }


                // =================================================
                // 4. RE-APPLY WAREHOUSE STOCK
                // =================================================

                foreach (var item in restoreItems)
                {
                    await _warehouseStockService
                        .IncreaseStockAsync(
                            item.WarehouseId,
                            item.ProductId,
                            item.Quantity);
                }


                // =================================================
                // 5. CREATE RESTORE LEDGER
                // =================================================

                StockTransactionType restoreType =
                    receipt.ReceiptType ==
                        GoodsReceiptType.Gift

                        ? StockTransactionType
                            .GiftReceipt

                        : StockTransactionType
                            .PurchaseReceipt;


                string restoreReference =
                    $"RESTORE-{receipt.GRNNumber}";


                foreach (var item in restoreItems)
                {
                    await _stockLedgerService
                        .AddStockMovementAsync(
                            productId:
                                item.ProductId,

                            warehouseId:
                                item.WarehouseId,

                            quantity:
                                item.Quantity,

                            transactionType:
                                restoreType,

                            unitCost:
                                item.UnitCost,

                            referenceNo:
                                restoreReference,

                            remarks:
                                $"Stock restored for GRN {receipt.GRNNumber}");
                }


                // =================================================
                // 6. UNDELETE GRN
                // =================================================

                await _repository
                    .RestoreAsync(id);


                // =================================================
                // 7. RECALCULATE PO STATUS
                // =================================================

                if (receipt.ReceiptType ==
                        GoodsReceiptType.Purchase &&
                    receipt.PurchaseOrderId.HasValue)
                {
                    await _repository
                        .RecalculatePurchaseOrderStatusAsync(
                            receipt.PurchaseOrderId.Value);
                }


                // =================================================
                // 8. SAVE EVERYTHING
                // =================================================

                await _unitOfWork
                    .SaveChangesAsync();


                // =================================================
                // 9. COMMIT
                // =================================================

                await _unitOfWork
                    .CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork
                    .RollbackAsync();

                throw;
            }
        }
    }
}