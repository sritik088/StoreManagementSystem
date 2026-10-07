using Microsoft.EntityFrameworkCore;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesOrderRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<IEnumerable<SalesOrder>> GetAllAsync()
        {
            return await _context.SalesOrders
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.SalesOrderItems)
                    .ThenInclude(x => x.Product)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<SalesOrder?> GetByIdAsync(
            int id)
        {
            return await _context.SalesOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        // =========================================================
        // GET BY ID WITH DETAILS
        // =========================================================

        public async Task<SalesOrder?> GetByIdWithDetailsAsync(
            int id)
        {
            return await _context.SalesOrders
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.SalesOrderItems)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        // =========================================================
        // ADD
        // =========================================================

        public async Task<SalesOrder> AddAsync(
            SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException(
                    nameof(salesOrder));
            }

            await _context.SalesOrders.AddAsync(
                salesOrder);

            await _context.SaveChangesAsync();

            return salesOrder;
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task UpdateAsync(
            SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException(
                    nameof(salesOrder));
            }

            if (salesOrder.Id <= 0)
            {
                throw new ArgumentException(
                    "Invalid Sales Order ID.",
                    nameof(salesOrder));
            }

            // -----------------------------------------------------
            // IMPORTANT:
            // Remove any SalesOrderItem entities that may already
            // be tracked by this DbContext.
            //
            // This prevents the:
            //
            // "SalesOrderItem.Id has a temporary value while
            // attempting to change the entity's state to Deleted"
            //
            // exception.
            // -----------------------------------------------------

            var trackedItems =
                _context.ChangeTracker
                    .Entries<SalesOrderItem>()
                    .ToList();

            foreach (var entry in trackedItems)
            {
                entry.State = EntityState.Detached;
            }

            // -----------------------------------------------------
            // Detach an already tracked SalesOrder if one exists.
            // -----------------------------------------------------

            var trackedOrders =
                _context.ChangeTracker
                    .Entries<SalesOrder>()
                    .Where(x =>
                        x.Entity.Id == salesOrder.Id)
                    .ToList();

            foreach (var entry in trackedOrders)
            {
                entry.State = EntityState.Detached;
            }

            // -----------------------------------------------------
            // Load ONLY the existing SalesOrder header.
            //
            // Do NOT Include SalesOrderItems here.
            // -----------------------------------------------------

            var existingOrder =
                await _context.SalesOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == salesOrder.Id &&
                        !x.IsDeleted);

            if (existingOrder == null)
            {
                throw new InvalidOperationException(
                    $"Sales Order with ID {salesOrder.Id} was not found.");
            }

            // -----------------------------------------------------
            // Update header fields
            // -----------------------------------------------------

            existingOrder.OrderNumber =
                salesOrder.OrderNumber;

            existingOrder.OrderDate =
                salesOrder.OrderDate;

            existingOrder.WarehouseId =
                salesOrder.WarehouseId;

            existingOrder.CustomerName =
                salesOrder.CustomerName;

            existingOrder.CustomerPhone =
                salesOrder.CustomerPhone;

            existingOrder.Remarks =
                salesOrder.Remarks;

            existingOrder.Status =
                salesOrder.Status;

            existingOrder.SubTotal =
                salesOrder.SubTotal;

            existingOrder.DiscountAmount =
                salesOrder.DiscountAmount;

            existingOrder.TaxAmount =
                salesOrder.TaxAmount;

            existingOrder.GrandTotal =
                salesOrder.GrandTotal;

            existingOrder.GrossProfit =
                salesOrder.GrossProfit;

            existingOrder.UpdatedDate =
                salesOrder.UpdatedDate ??
                DateTime.Now;

            // -----------------------------------------------------
            // Prepare NEW SalesOrderItems.
            //
            // IMPORTANT:
            // We intentionally do NOT copy Id.
            // SQL Server/EF Core must generate the new Id.
            // -----------------------------------------------------

            var newItems =
                salesOrder.SalesOrderItems?
                    .Where(x =>
                        x.ProductId > 0 &&
                        x.Quantity > 0)
                    .Select(item =>
                        new SalesOrderItem
                        {
                            SalesOrderId =
                                existingOrder.Id,

                            ProductId =
                                item.ProductId,

                            Quantity =
                                item.Quantity,

                            // Actual purchase / GRN cost
                            UnitPrice =
                                item.UnitPrice,

                            // Customer selling price
                            SalePrice =
                                item.SalePrice,

                            DiscountPercent =
                                item.DiscountPercent,

                            TaxPercent =
                                item.TaxPercent,

                            LineTotal =
                                item.LineTotal,

                            GrossProfit =
                                item.GrossProfit
                        })
                    .ToList()
                ?? new List<SalesOrderItem>();

            // -----------------------------------------------------
            // Load OLD SalesOrderItems directly from database.
            //
            // Do NOT use existingOrder.SalesOrderItems.
            // -----------------------------------------------------

            var oldItems =
                await _context.SalesOrderItems
                    .Where(x =>
                        x.SalesOrderId ==
                        existingOrder.Id)
                    .ToListAsync();

            // -----------------------------------------------------
            // Delete OLD items
            // -----------------------------------------------------

            if (oldItems.Count > 0)
            {
                _context.SalesOrderItems
                    .RemoveRange(oldItems);

                // Save the deletion BEFORE inserting new items.
                await _context.SaveChangesAsync();
            }

            // -----------------------------------------------------
            // Add NEW items
            // -----------------------------------------------------

            if (newItems.Count > 0)
            {
                await _context.SalesOrderItems
                    .AddRangeAsync(newItems);
            }

            // -----------------------------------------------------
            // Save header + new items
            // -----------------------------------------------------

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // EXISTS
        // =========================================================

        public async Task<bool> ExistsAsync(
            int id)
        {
            return await _context.SalesOrders
                .AnyAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        // =========================================================
        // GET LATEST COST PRICE
        // =========================================================

        public async Task<decimal?> GetLatestCostPriceAsync(
            int productId)
        {
            var item =
                await _context.GoodsReceiptItems
                    .AsNoTracking()
                    .Include(x => x.GoodsReceipt)
                    .Where(x =>
                        x.ProductId == productId &&
                        x.GoodsReceipt != null &&
                        !x.GoodsReceipt.IsDeleted &&
                        x.GoodsReceipt.Status ==
                            GoodsReceiptStatus.Received &&
                        x.GoodsReceipt.ReceiptType ==
                            GoodsReceiptType.Purchase &&
                        x.ReceivedQuantity > 0)
                    .OrderByDescending(x =>
                        x.GoodsReceipt!.ReceiptDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .FirstOrDefaultAsync();

            return item?.UnitPrice;
        }
    }
}