using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class PurchaseOrderRepository
        : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL
        // =====================================================
        //
        // IMPORTANT:
        // Deleted Purchase Orders are intentionally included.
        //
        // This allows the Index page to show deleted POs
        // with a Restore button.
        //
        // =====================================================

        public async Task<IEnumerable<PurchaseOrder>>
            GetAllAsync()
        {
            return await _context.PurchaseOrders

                .Include(x => x.Supplier)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .OrderByDescending(x => x.OrderDate)

                .AsNoTracking()

                .ToListAsync();
        }


        // =====================================================
        // GET LATEST UNIT PRICE FOR PRODUCT
        // =====================================================

        public async Task<decimal>
            GetLatestUnitPriceAsync(
                int productId)
        {
            var latestItem =
                await _context.PurchaseOrderItems

                    .Include(x => x.PurchaseOrder)

                    .Where(x =>
                        x.ProductId == productId &&

                        x.PurchaseOrder != null &&

                        !x.PurchaseOrder.IsDeleted &&

                        x.PurchaseOrder.Status !=
                            PurchaseOrderStatus.Draft &&

                        x.PurchaseOrder.Status !=
                            PurchaseOrderStatus.Cancelled)

                    .OrderByDescending(x =>
                        x.PurchaseOrder!.OrderDate)

                    .ThenByDescending(x =>
                        x.PurchaseOrder!.Id)

                    .FirstOrDefaultAsync();

            return latestItem?.UnitPrice ?? 0m;
        }


        // =====================================================
        // GET BY ID
        // =====================================================
        //
        // IMPORTANT:
        // Do NOT filter IsDeleted here.
        //
        // Restore needs to load a deleted Purchase Order.
        //
        // =====================================================

        public async Task<PurchaseOrder?>
            GetByIdAsync(int id)
        {
            return await _context.PurchaseOrders

                .Include(x => x.Supplier)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .FirstOrDefaultAsync(x =>
                    x.Id == id);
        }


        // =====================================================
        // GET WITH DETAILS
        // =====================================================

        public async Task<PurchaseOrder?>
            GetByIdWithDetailsAsync(int id)
        {
            return await _context.PurchaseOrders

                .Include(x => x.Supplier)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .AsNoTracking()

                .FirstOrDefaultAsync(x =>
                    x.Id == id);
        }


        // =====================================================
        // GENERATE PO NUMBER
        // =====================================================

        public async Task<string>
            GeneratePONumberAsync()
        {
            string prefix =
                $"PO-{DateTime.Now:yyyyMMdd}-";

            var lastPO =
                await _context.PurchaseOrders

                    .Where(x =>
                        x.PONumber.StartsWith(prefix))

                    .OrderByDescending(x => x.Id)

                    .FirstOrDefaultAsync();

            int next = 1;

            if (lastPO != null)
            {
                var number =
                    lastPO.PONumber
                        .Substring(prefix.Length);

                if (int.TryParse(
                    number,
                    out int lastNumber))
                {
                    next = lastNumber + 1;
                }
            }

            return $"{prefix}{next:0000}";
        }


        // =====================================================
        // ADD
        // =====================================================

        public async Task AddAsync(
            PurchaseOrder purchaseOrder)
        {
            await _context.PurchaseOrders
                .AddAsync(purchaseOrder);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(
            PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders
                .Update(purchaseOrder);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // CHECK ACTIVE GOODS RECEIPTS
        // =====================================================

        public async Task<bool>
            HasActiveGoodsReceiptsAsync(
                int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
                return false;

            return await _context.GoodsReceipts
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PurchaseOrderId ==
                        purchaseOrderId &&

                    !x.IsDeleted);
        }


        // =====================================================
        // DELETE - SOFT DELETE
        // =====================================================

        public async Task DeleteAsync(int id)
        {
            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var purchaseOrder =
                    await _context.PurchaseOrders

                        .Include(x => x.Items)

                        .FirstOrDefaultAsync(x =>
                            x.Id == id);

                if (purchaseOrder == null)
                {
                    await transaction.RollbackAsync();
                    return;
                }

                // Already deleted
                if (purchaseOrder.IsDeleted)
                {
                    await transaction.RollbackAsync();
                    return;
                }

                // -------------------------------------------------
                // FINAL ACTIVE GRN CHECK
                // -------------------------------------------------

                var hasActiveGRN =
                    await _context.GoodsReceipts
                        .AnyAsync(x =>
                            x.PurchaseOrderId == id &&
                            !x.IsDeleted);

                if (hasActiveGRN)
                {
                    await transaction.RollbackAsync();
                    return;
                }

                // -------------------------------------------------
                // SOFT DELETE
                // -------------------------------------------------

                purchaseOrder.IsDeleted = true;

                purchaseOrder.UpdatedDate =
                    DateTime.Now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // =====================================================
        // RESTORE
        // =====================================================

        public async Task RestoreAsync(int id)
        {
            var purchaseOrder =
                await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (purchaseOrder == null)
                return;

            // Already active
            if (!purchaseOrder.IsDeleted)
                return;

            // -------------------------------------------------
            // RESTORE
            // -------------------------------------------------

            purchaseOrder.IsDeleted = false;

            purchaseOrder.UpdatedDate =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // APPROVE
        // =====================================================

        public async Task ApproveAsync(int id)
        {
            var purchaseOrder =
                await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

            if (purchaseOrder == null)
                return;

            if (purchaseOrder.Status !=
                PurchaseOrderStatus.Draft)
            {
                return;
            }

            purchaseOrder.Status =
                PurchaseOrderStatus.Approved;

            purchaseOrder.UpdatedDate =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // CANCEL
        // =====================================================

        public async Task CancelAsync(int id)
        {
            var purchaseOrder =
                await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

            if (purchaseOrder == null)
                return;

            if (purchaseOrder.Status ==
                    PurchaseOrderStatus.PartiallyReceived ||
                purchaseOrder.Status ==
                    PurchaseOrderStatus.FullyReceived)
            {
                return;
            }

            purchaseOrder.Status =
                PurchaseOrderStatus.Cancelled;

            purchaseOrder.UpdatedDate =
                DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}