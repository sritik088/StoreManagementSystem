using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class StockLedgerRepository
        : IStockLedgerRepository
    {
        private readonly ApplicationDbContext _context;

        public StockLedgerRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // ADD LEDGER ENTRY
        // =====================================================

        public async Task AddAsync(
            StockLedger ledger)
        {
            if (ledger == null)
                throw new ArgumentNullException(
                    nameof(ledger));

            if (ledger.ProductId <= 0)
                throw new ArgumentException(
                    "ProductId is required.");

            if (ledger.WarehouseId <= 0)
                throw new ArgumentException(
                    "WarehouseId is required.");

            if (ledger.Quantity == 0)
                throw new ArgumentException(
                    "Ledger quantity cannot be zero.");

            if (string.IsNullOrWhiteSpace(
                ledger.ReferenceNo))
            {
                throw new ArgumentException(
                    "Reference number is required.");
            }

            await _context.StockLedgers
                .AddAsync(ledger);

            // IMPORTANT:
            // Do NOT call SaveChangesAsync() here.
            //
            // UnitOfWork will commit the entire transaction.
        }

        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<StockLedger>>
            GetAllAsync()
        {
            return await _context.StockLedgers

                .Include(x => x.Product)

                .Include(x => x.Warehouse)

                .OrderByDescending(
                    x => x.TransactionDate)

                .ThenByDescending(
                    x => x.Id)

                .AsNoTracking()

                .ToListAsync();
        }

        // =====================================================
        // GET BY PRODUCT
        // =====================================================

        public async Task<IEnumerable<StockLedger>>
            GetByProductAsync(
                int productId)
        {
            return await _context.StockLedgers

                .Include(x => x.Product)

                .Include(x => x.Warehouse)

                .Where(x =>
                    x.ProductId == productId)

                .OrderByDescending(
                    x => x.TransactionDate)

                .ThenByDescending(
                    x => x.Id)

                .AsNoTracking()

                .ToListAsync();
        }

        // =====================================================
        // GET BY WAREHOUSE
        // =====================================================

        public async Task<IEnumerable<StockLedger>>
            GetByWarehouseAsync(
                int warehouseId)
        {
            return await _context.StockLedgers

                .Include(x => x.Product)

                .Include(x => x.Warehouse)

                .Where(x =>
                    x.WarehouseId == warehouseId)

                .OrderByDescending(
                    x => x.TransactionDate)

                .ThenByDescending(
                    x => x.Id)

                .AsNoTracking()

                .ToListAsync();
        }

        // =====================================================
        // PRODUCT + WAREHOUSE
        // =====================================================

        public async Task<IEnumerable<StockLedger>>
            GetByProductAndWarehouseAsync(
                int productId,
                int warehouseId)
        {
            return await _context.StockLedgers

                .Include(x => x.Product)

                .Include(x => x.Warehouse)

                .Where(x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId)

                .OrderBy(
                    x => x.TransactionDate)

                .ThenBy(
                    x => x.Id)

                .AsNoTracking()

                .ToListAsync();
        }

        // =====================================================
        // CURRENT BALANCE
        // =====================================================

        public async Task<decimal>
            GetCurrentBalanceAsync(
                int productId,
                int warehouseId)
        {
            var lastEntry =
                await _context.StockLedgers

                    .Where(x =>
                        x.ProductId == productId &&
                        x.WarehouseId == warehouseId)

                    .OrderByDescending(
                        x => x.TransactionDate)

                    .ThenByDescending(
                        x => x.Id)

                    .Select(x =>
                        (decimal?)x.BalanceAfterTransaction)

                    .FirstOrDefaultAsync();

            return lastEntry ?? 0m;
        }

        // =====================================================
        // LAST ENTRY
        // =====================================================

        public async Task<StockLedger?>
            GetLastEntryAsync(
                int productId,
                int warehouseId)
        {
            return await _context.StockLedgers

                .Where(x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId)

                .OrderByDescending(
                    x => x.TransactionDate)

                .ThenByDescending(
                    x => x.Id)

                .AsNoTracking()

                .FirstOrDefaultAsync();
        }
    }
}