using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _transferRepository;
    private readonly IWarehouseStockService _warehouseStockService;
    private readonly IStockLedgerRepository _ledgerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockTransferService(
        IStockTransferRepository transferRepository,
        IWarehouseStockService warehouseStockService,
        IStockLedgerRepository ledgerRepository,
        IUnitOfWork unitOfWork)
    {
        _transferRepository = transferRepository;
        _warehouseStockService = warehouseStockService;
        _ledgerRepository = ledgerRepository;
        _unitOfWork = unitOfWork;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<List<StockTransfer>> GetAllAsync()
    {
        return await _transferRepository.GetAllAsync();
    }

    // =========================================================
    // GET ACTIVE BY ID
    // =========================================================

    public async Task<StockTransfer?> GetByIdAsync(int id)
    {
        return await _transferRepository.GetByIdAsync(id);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<bool> CreateAsync(
        StockTransfer transfer)
    {
        if (transfer.FromWarehouseId ==
            transfer.ToWarehouseId)
        {
            throw new InvalidOperationException(
                "Source and destination warehouse cannot be the same.");
        }

        if (transfer.Items == null ||
            !transfer.Items.Any())
        {
            throw new InvalidOperationException(
                "Transfer must contain at least one item.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            transfer.TransferNumber =
                await _transferRepository
                    .GenerateTransferNumberAsync();

            transfer.TransferDate =
                DateTime.Now;

            transfer.Status =
                StockTransferStatus.Completed;

            transfer.CreatedDate =
                DateTime.Now;

            transfer.IsDeleted = false;

            // =====================================================
            // VALIDATE STOCK
            // =====================================================

            foreach (var item in transfer.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid quantity for Product ID {item.ProductId}.");
                }

                var available =
                    await _warehouseStockService
                        .GetAvailableStockAsync(
                            transfer.FromWarehouseId,
                            item.ProductId);

                if (available < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for Product ID {item.ProductId}. " +
                        $"Available: {available:N2}, " +
                        $"Requested: {item.Quantity:N2}.");
                }
            }

            // =====================================================
            // SAVE TRANSFER
            // =====================================================

            await _transferRepository
                .AddAsync(transfer);

            // =====================================================
            // UPDATE STOCK
            // =====================================================

            foreach (var item in transfer.Items)
            {
                await _warehouseStockService
                    .DecreaseStockAsync(
                        transfer.FromWarehouseId,
                        item.ProductId,
                        item.Quantity);

                await _warehouseStockService
                    .IncreaseStockAsync(
                        transfer.ToWarehouseId,
                        item.ProductId,
                        item.Quantity);
            }

            // =====================================================
            // LEDGER
            // =====================================================

            foreach (var item in transfer.Items)
            {
                await _ledgerRepository.AddAsync(
                    new StockLedger
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            transfer.FromWarehouseId,

                        TransactionType =
                            StockTransactionType.StockTransferOut,

                        Quantity =
                            -item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceNo =
                            transfer.TransferNumber,

                        TransactionDate =
                            DateTime.Now,

                        Remarks =
                            "Stock Transfer Out"
                    });

                await _ledgerRepository.AddAsync(
                    new StockLedger
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            transfer.ToWarehouseId,

                        TransactionType =
                            StockTransactionType.StockTransferIn,

                        Quantity =
                            item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceNo =
                            transfer.TransferNumber,

                        TransactionDate =
                            DateTime.Now,

                        Remarks =
                            "Stock Transfer In"
                    });
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

    // =========================================================
    // CANCEL / REVERSE
    // =========================================================

    public async Task<bool> CancelAsync(int id)
    {
        var transfer =
            await _transferRepository
                .GetByIdAsync(id);

        if (transfer == null)
            return false;

        if (transfer.Status ==
            StockTransferStatus.Cancelled)
        {
            return false;
        }

        if (transfer.Status !=
            StockTransferStatus.Completed)
        {
            return false;
        }

        if (transfer.Items == null ||
            !transfer.Items.Any())
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // =====================================================
            // CHECK DESTINATION STOCK
            // =====================================================

            foreach (var item in transfer.Items)
            {
                var destinationAvailable =
                    await _warehouseStockService
                        .GetAvailableStockAsync(
                            transfer.ToWarehouseId,
                            item.ProductId);

                if (destinationAvailable < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Transfer {transfer.TransferNumber} cannot be reversed. " +
                        $"Product ID {item.ProductId} has only " +
                        $"{destinationAvailable:N2} available stock in the destination warehouse, " +
                        $"but {item.Quantity:N2} is required for reversal.");
                }
            }

            // =====================================================
            // REVERSE STOCK
            // =====================================================

            foreach (var item in transfer.Items)
            {
                await _warehouseStockService
                    .DecreaseStockAsync(
                        transfer.ToWarehouseId,
                        item.ProductId,
                        item.Quantity);

                await _warehouseStockService
                    .IncreaseStockAsync(
                        transfer.FromWarehouseId,
                        item.ProductId,
                        item.Quantity);
            }

            // =====================================================
            // REVERSAL LEDGER
            // =====================================================

            foreach (var item in transfer.Items)
            {
                // Destination
                await _ledgerRepository.AddAsync(
                    new StockLedger
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            transfer.ToWarehouseId,

                        TransactionType =
                            StockTransactionType.StockTransferIn,

                        Quantity =
                            -item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceNo =
                            transfer.TransferNumber,

                        TransactionDate =
                            DateTime.Now,

                        Remarks =
                            "Stock Transfer Reversal - Destination"
                    });

                // Source
                await _ledgerRepository.AddAsync(
                    new StockLedger
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            transfer.FromWarehouseId,

                        TransactionType =
                            StockTransactionType.StockTransferOut,

                        Quantity =
                            item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceNo =
                            transfer.TransferNumber,

                        TransactionDate =
                            DateTime.Now,

                        Remarks =
                            "Stock Transfer Reversal - Source"
                    });
            }

            // =====================================================
            // CANCEL
            // =====================================================

            transfer.Status =
                StockTransferStatus.Cancelled;

            transfer.UpdatedDate =
                DateTime.Now;

            _transferRepository.Update(
                transfer);

            await _unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // DELETE / REVERSE + SOFT DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var transfer =
            await _transferRepository
                .GetByIdAsync(id);

        if (transfer == null)
            return false;

        // ---------------------------------------------------------
        // Completed transfer:
        // Reverse stock first.
        // ---------------------------------------------------------

        if (transfer.Status ==
            StockTransferStatus.Completed)
        {
            var reversed =
                await CancelAsync(id);

            if (!reversed)
                return false;

            transfer =
                await _transferRepository
                    .GetByIdAsync(id);

            if (transfer == null)
                return false;
        }

        // ---------------------------------------------------------
        // Only cancelled transfer can be soft deleted.
        // ---------------------------------------------------------

        if (transfer.Status ==
            StockTransferStatus.Cancelled)
        {
            return await _transferRepository
                .DeleteAsync(id);
        }

        return false;
    }

    // =========================================================
    // RESTORE
    // =========================================================

    public async Task<bool> RestoreAsync(int id)
    {
        var transfer =
            await _transferRepository
                .GetDeletedByIdAsync(id);

        if (transfer == null)
            return false;

        if (!transfer.IsDeleted)
            return false;

        if (transfer.Status !=
            StockTransferStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Stock Transfer {transfer.TransferNumber} " +
                "cannot be restored because its status is not Cancelled.");
        }

        if (transfer.Items == null ||
            !transfer.Items.Any())
        {
            throw new InvalidOperationException(
                $"Stock Transfer {transfer.TransferNumber} " +
                "does not contain any items.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // =====================================================
            // CHECK DESTINATION STOCK
            // =====================================================

            foreach (var item in transfer.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid quantity for Product ID {item.ProductId}.");
                }
            }

            // =====================================================
            // RESTORE STOCK
            // =====================================================

            foreach (var item in transfer.Items)
            {
                // Remove stock from source again
                await _warehouseStockService
                    .DecreaseStockAsync(
                        transfer.FromWarehouseId,
                        item.ProductId,
                        item.Quantity);

                // Add stock to destination again
                await _warehouseStockService
                    .IncreaseStockAsync(
                        transfer.ToWarehouseId,
                        item.ProductId,
                        item.Quantity);
            }

            // =====================================================
            // RESTORE LEDGER
            // =====================================================

            foreach (var item in transfer.Items)
            {
                // Source
                await _ledgerRepository.AddAsync(
                    new StockLedger
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            transfer.FromWarehouseId,

                        TransactionType =
                            StockTransactionType.StockTransferOut,

                        Quantity =
                            -item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceNo =
                            $"RESTORE-{transfer.TransferNumber}",

                        TransactionDate =
                            DateTime.Now,

                        Remarks =
                            "Stock Transfer Restored - Source"
                    });

                // Destination
                await _ledgerRepository.AddAsync(
                    new StockLedger
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            transfer.ToWarehouseId,

                        TransactionType =
                            StockTransactionType.StockTransferIn,

                        Quantity =
                            item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceNo =
                            $"RESTORE-{transfer.TransferNumber}",

                        TransactionDate =
                            DateTime.Now,

                        Remarks =
                            "Stock Transfer Restored - Destination"
                    });
            }

            // =====================================================
            // RESTORE TRANSFER
            // =====================================================

            transfer.IsDeleted = false;

            transfer.Status =
                StockTransferStatus.Completed;

            transfer.UpdatedDate =
                DateTime.Now;

            _transferRepository.Update(
                transfer);

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