
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class WarehouseStockRepository : IWarehouseStockRepository
    {
        private readonly ApplicationDbContext _context;

        public WarehouseStockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET STOCK BY WAREHOUSE + PRODUCT
        // =========================================================

        public async Task<WarehouseStock?> GetAsync(
            int warehouseId,
            int productId)
        {
            return await _context.WarehouseStocks
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouseId &&
                    x.ProductId == productId);
        }

        // =========================================================
        // GET STOCK BY PRODUCT
        // =========================================================

        public async Task<List<WarehouseStock>> GetByProductAsync(
            int productId)
        {
            return await _context.WarehouseStocks
                .Include(x => x.Warehouse)
                .Where(x => x.ProductId == productId)
                .ToListAsync();
        }

        // =========================================================
        // GET STOCK BY WAREHOUSE
        // =========================================================

        public async Task<List<WarehouseStock>> GetByWarehouseAsync(
            int warehouseId)
        {
            return await _context.WarehouseStocks
                .Include(x => x.Product)
                .Where(x => x.WarehouseId == warehouseId)
                .ToListAsync();
        }

        // =========================================================
        // EXISTS
        // =========================================================

        public async Task<bool> ExistsAsync(
            int warehouseId,
            int productId)
        {
            return await _context.WarehouseStocks
                .AnyAsync(x =>
                    x.WarehouseId == warehouseId &&
                    x.ProductId == productId);
        }

        // =========================================================
        // ADD
        // =========================================================

        public async Task AddAsync(WarehouseStock stock)
        {
            if (stock == null)
                throw new ArgumentNullException(nameof(stock));

            await _context.WarehouseStocks.AddAsync(stock);
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public void Update(WarehouseStock stock)
        {
            if (stock == null)
                throw new ArgumentNullException(nameof(stock));

            _context.WarehouseStocks.Update(stock);
        }

        // =========================================================
        // INCREASE STOCK
        // =========================================================

        public async Task IncreaseStockAsync(
            int warehouseId,
            int productId,
            decimal quantity)
        {
            if (warehouseId <= 0)
                throw new InvalidOperationException(
                    "Invalid warehouse.");

            if (productId <= 0)
                throw new InvalidOperationException(
                    "Invalid product.");

            if (quantity <= 0)
                throw new InvalidOperationException(
                    "Stock quantity must be greater than zero.");

            var stock = await GetAsync(
                warehouseId,
                productId);

            // -----------------------------------------------------
            // EXISTING STOCK
            // -----------------------------------------------------

            if (stock != null)
            {
                stock.QuantityOnHand += quantity;
                stock.LastUpdated = DateTime.Now;

                Update(stock);

                return;
            }

            // -----------------------------------------------------
            // NEW STOCK
            // -----------------------------------------------------

            var newStock = new WarehouseStock
            {
                WarehouseId = warehouseId,
                ProductId = productId,

                QuantityOnHand = quantity,

                ReservedQuantity = 0m,

                MinimumStock = 0m,

                MaximumStock = 0m,

                LastUpdated = DateTime.Now
            };

            await AddAsync(newStock);
        }

        // =========================================================
        // DECREASE STOCK
        // =========================================================

        public async Task DecreaseStockAsync(
            int warehouseId,
            int productId,
            decimal quantity)
        {
            if (warehouseId <= 0)
                throw new InvalidOperationException(
                    "Invalid warehouse.");

            if (productId <= 0)
                throw new InvalidOperationException(
                    "Invalid product.");

            if (quantity <= 0)
                throw new InvalidOperationException(
                    "Stock quantity must be greater than zero.");

            var stock = await GetAsync(
                warehouseId,
                productId);

            if (stock == null)
                throw new InvalidOperationException(
                    "Warehouse stock not found.");

            if (stock.QuantityOnHand < quantity)
                throw new InvalidOperationException(
                    "Insufficient warehouse stock.");

            stock.QuantityOnHand -= quantity;

            stock.LastUpdated = DateTime.Now;

            Update(stock);
        }
      
public async Task<List<WarehouseStock>> GetAllAsync()
        {
            return await _context.WarehouseStocks
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .OrderBy(x => x.WarehouseId)
                .ThenBy(x => x.ProductId)
                .ToListAsync();
        }


    }
}

