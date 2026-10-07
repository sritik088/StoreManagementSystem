using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class GoodsReceiptRepository : IGoodsReceiptRepository
    {
        private readonly ApplicationDbContext _context;

        public GoodsReceiptRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL GOODS RECEIPTS
        // =====================================================
        // IMPORTANT:
        // Includes BOTH active and deleted/reversed GRNs.
        // This is required so the Index page can show the
        // Restore button for deleted GRNs.
        // =====================================================

        public async Task<IEnumerable<GoodsReceipt>>
            GetAllAsync()
        {
            return await _context.GoodsReceipts

                .Include(x => x.Supplier)

                .Include(x => x.PurchaseOrder)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Warehouse)

                .OrderByDescending(x => x.ReceiptDate)

                .ThenByDescending(x => x.Id)

                .AsNoTracking()

                .ToListAsync();
        }

        // =====================================================
        // GET DELETED GOODS RECEIPT
        // =====================================================
        // Used only when restoring a deleted GRN.
        // =====================================================

        public async Task<GoodsReceipt?>
            GetDeletedByIdAsync(int id)
        {
            return await _context.GoodsReceipts

                .Include(x => x.Supplier)

                .Include(x => x.PurchaseOrder)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Warehouse)

                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted);
        }

        // =====================================================
        // RESTORE GOODS RECEIPT
        // =====================================================
        // IMPORTANT:
        // Only changes the GRN from Deleted -> Active.
        //
        // Stock restoration and stock ledger entry are handled
        // by GoodsReceiptService.RestoreAsync() inside the
        // UnitOfWork transaction.
        // =====================================================

        public async Task RestoreAsync(int id)
        {
            var receipt =
                await _context.GoodsReceipts
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.IsDeleted);

            if (receipt == null)
                return;

            receipt.IsDeleted = false;

            receipt.UpdatedDate =
                DateTime.Now;

            // IMPORTANT:
            // Do NOT call SaveChangesAsync().
            // UnitOfWork controls the transaction.
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        // IMPORTANT:
        // This intentionally returns ONLY active GRNs.
        //
        // DeleteAsync() uses this method to prevent deleting
        // an already deleted GRN a second time.
        // =====================================================

        public async Task<GoodsReceipt?>
            GetByIdAsync(int id)
        {
            return await _context.GoodsReceipts

                .Include(x => x.Supplier)

                .Include(x => x.PurchaseOrder)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Warehouse)

                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        // =====================================================
        // GET PURCHASE ORDER WITH DETAILS
        // =====================================================

        public async Task<PurchaseOrder?>
            GetPurchaseOrderWithDetailsAsync(
                int id)
        {
            return await _context.PurchaseOrders

                .Include(x => x.Supplier)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .AsNoTracking()

                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        // =====================================================
        // GENERATE GRN NUMBER
        // =====================================================

        public async Task<string>
            GenerateGRNNumberAsync()
        {
            string prefix =
                $"GRN-{DateTime.Now:yyyyMMdd}-";

            var last =
                await _context.GoodsReceipts

                    .Where(x =>
                        x.GRNNumber.StartsWith(prefix))

                    .OrderByDescending(x => x.Id)

                    .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var number =
                    last.GRNNumber
                        .Substring(prefix.Length);

                if (int.TryParse(
                    number,
                    out int lastNumber))
                {
                    next =
                        lastNumber + 1;
                }
            }

            return
                $"{prefix}{next:0000}";
        }

        // =====================================================
        // APPROVED / PARTIALLY RECEIVED PURCHASE ORDERS
        // =====================================================

        public async Task<IEnumerable<PurchaseOrder>>
            GetApprovedPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders

                .Include(x => x.Supplier)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.Status ==
                            PurchaseOrderStatus.Approved ||

                        x.Status ==
                            PurchaseOrderStatus.PartiallyReceived
                    ))

                .OrderByDescending(x => x.OrderDate)

                .ToListAsync();
        }

        // =====================================================
        // PREVIOUSLY RECEIVED QUANTITY
        // =====================================================

        public async Task<decimal>
            GetPreviouslyReceivedQuantityAsync(
                int purchaseOrderItemId)
        {
            return await _context.GoodsReceiptItems

                .Where(x =>
                    x.PurchaseOrderItemId ==
                        purchaseOrderItemId &&

                    x.GoodsReceipt != null &&

                    !x.GoodsReceipt.IsDeleted)

                .SumAsync(x =>
                    (decimal?)x.ReceivedQuantity)

                ?? 0m;
        }

        // =====================================================
        // ADD
        // =====================================================

        public async Task AddAsync(
            GoodsReceipt receipt)
        {
            if (receipt == null)
                throw new ArgumentNullException(
                    nameof(receipt));

            await _context.GoodsReceipts
                .AddAsync(receipt);

            // IMPORTANT:
            // No SaveChanges here.
            // UnitOfWork controls the transaction.
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(
            GoodsReceipt receipt)
        {
            if (receipt == null)
                throw new ArgumentNullException(
                    nameof(receipt));

            _context.GoodsReceipts
                .Update(receipt);

            // IMPORTANT:
            // No SaveChanges here.
            // UnitOfWork controls the transaction.
        }

        // =====================================================
        // SOFT DELETE
        // =====================================================
        // The GRN is NOT permanently removed.
        //
        // GoodsReceiptService.DeleteAsync() performs:
        //
        // 1. Validate stock
        // 2. Decrease warehouse stock
        // 3. Create reversal ledger
        // 4. Recalculate PO status
        // 5. Soft delete GRN
        //
        // All inside one transaction.
        // =====================================================

        public async Task DeleteAsync(int id)
        {
            var receipt =
                await _context.GoodsReceipts
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

            if (receipt == null)
                return;

            receipt.IsDeleted = true;

            receipt.UpdatedDate =
                DateTime.Now;

            // IMPORTANT:
            // Do NOT call SaveChangesAsync().
            // UnitOfWork controls the transaction.
        }

        // =====================================================
        // RECALCULATE PURCHASE ORDER STATUS
        // =====================================================

        public async Task
            RecalculatePurchaseOrderStatusAsync(
                int purchaseOrderId)
        {
            var purchaseOrder =
                await _context.PurchaseOrders

                    .Include(x => x.Items)

                    .FirstOrDefaultAsync(x =>
                        x.Id == purchaseOrderId &&
                        !x.IsDeleted);

            if (purchaseOrder == null)
                return;

            bool hasAnyReceipt = false;

            bool fullyReceived = true;

            foreach (var poItem in purchaseOrder.Items)
            {
                decimal receivedQuantity =
                    await _context.GoodsReceiptItems

                        .Where(x =>
                            x.PurchaseOrderItemId ==
                                poItem.Id &&

                            x.GoodsReceipt != null &&

                            !x.GoodsReceipt.IsDeleted)

                        .SumAsync(x =>
                            (decimal?)x.ReceivedQuantity)

                        ?? 0m;

                if (receivedQuantity > 0)
                {
                    hasAnyReceipt = true;
                }

                if (receivedQuantity < poItem.Quantity)
                {
                    fullyReceived = false;
                }
            }

            // =================================================
            // UPDATE PURCHASE ORDER STATUS
            // =================================================

            if (!hasAnyReceipt)
            {
                purchaseOrder.Status =
                    PurchaseOrderStatus.Approved;
            }
            else if (fullyReceived)
            {
                purchaseOrder.Status =
                    PurchaseOrderStatus.FullyReceived;
            }
            else
            {
                purchaseOrder.Status =
                    PurchaseOrderStatus.PartiallyReceived;
            }

            purchaseOrder.UpdatedDate =
                DateTime.Now;

            // Entity is tracked.
            // UnitOfWork will save it.
        }
    }
}