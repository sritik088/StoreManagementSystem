using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories;

public class StockTransferRepository
    : IStockTransferRepository
{
    private readonly ApplicationDbContext _context;

    public StockTransferRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockTransfer>> GetAllAsync()
    {
        return await _context.StockTransfers

            .Include(x => x.FromWarehouse)

            .Include(x => x.ToWarehouse)

            .Include(x => x.Items)
                .ThenInclude(i => i.Product)

            .Where(x => !x.IsDeleted)

            .OrderByDescending(x => x.TransferDate)

            .ToListAsync();
    }

    public async Task<StockTransfer?> GetByIdAsync(int id)
    {
        return await _context.StockTransfers

            .Include(x => x.FromWarehouse)

            .Include(x => x.ToWarehouse)

            .Include(x => x.Items)
                .ThenInclude(i => i.Product)

            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);
    }

    public async Task<string>
GenerateTransferNumberAsync()
    {
        string prefix =
            $"ST-{DateTime.Now:yyyyMMdd}-";

        var last =
            await _context.StockTransfers

            .Where(x =>
                x.TransferNumber.StartsWith(prefix))

            .OrderByDescending(x => x.Id)

            .FirstOrDefaultAsync();

        int next = 1;

        if (last != null)
        {
            string number =
                last.TransferNumber
                    .Substring(prefix.Length);

            int.TryParse(number, out next);

            next++;
        }

        return $"{prefix}{next:0000}";
    }

    public async Task AddAsync(
    StockTransfer transfer)
    {
        await _context.StockTransfers
            .AddAsync(transfer);
    }

    public void Update(
    StockTransfer transfer)
    {
        _context.StockTransfers
            .Update(transfer);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.StockTransfers
            .AnyAsync(x => x.Id == id);
    }

}
