using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories;

public class WarehouseStockRepository : IWarehouseStockRepository
{
    private readonly ApplicationDbContext _context;

    public WarehouseStockRepository(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public async Task<List<WarehouseStock>> GetByProductAsync(int productId)
    {
        return await _context.WarehouseStocks
            .Where(x => x.ProductId == productId)
            .ToListAsync();
    }

    public async Task<List<WarehouseStock>> GetByWarehouseAsync(int warehouseId)
    {
        return await _context.WarehouseStocks
            .Include(x => x.Product)
            .Where(x => x.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(
        int warehouseId,
        int productId)
    {
        return await _context.WarehouseStocks.AnyAsync(x =>
            x.WarehouseId == warehouseId &&
            x.ProductId == productId);
    }

    public async Task AddAsync(WarehouseStock stock)
    {
        await _context.WarehouseStocks.AddAsync(stock);
    }

    public void Update(WarehouseStock stock)
    {
        _context.WarehouseStocks.Update(stock);
    }
}