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

    // =========================================================
    // GET ALL
    // Includes ACTIVE + DELETED transfers
    // =========================================================

    public async Task<List<StockTransfer>> GetAllAsync()
    {
        return await _context.StockTransfers

            .Include(x => x.FromWarehouse)

            .Include(x => x.ToWarehouse)

            .Include(x => x.Items)
                .ThenInclude(i => i.Product)

            .OrderByDescending(x => x.TransferDate)

            .ToListAsync();
    }

    // =========================================================
    // GET ACTIVE TRANSFER
    // =========================================================

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

    // =========================================================
    // GET DELETED TRANSFER
    // =========================================================

    public async Task<StockTransfer?> GetDeletedByIdAsync(int id)
    {
        return await _context.StockTransfers

            .Include(x => x.FromWarehouse)

            .Include(x => x.ToWarehouse)

            .Include(x => x.Items)
                .ThenInclude(i => i.Product)

            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsDeleted);
    }

    // =========================================================
    // GENERATE TRANSFER NUMBER
    // =========================================================

    public async Task<string> GenerateTransferNumberAsync()
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

            if (int.TryParse(number, out int lastNumber))
            {
                next = lastNumber + 1;
            }
        }

        return $"{prefix}{next:0000}";
    }

    // =========================================================
    // ADD
    // =========================================================

    public async Task AddAsync(
        StockTransfer transfer)
    {
        await _context.StockTransfers
            .AddAsync(transfer);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public void Update(
        StockTransfer transfer)
    {
        _context.StockTransfers
            .Update(transfer);
    }

    // =========================================================
    // EXISTS
    // =========================================================

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.StockTransfers
            .AnyAsync(x =>
                x.Id == id &&
                !x.IsDeleted);
    }

    // =========================================================
    // SOFT DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var transfer =
            await _context.StockTransfers
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (transfer == null)
            return false;

        transfer.IsDeleted = true;

        transfer.UpdatedDate =
            DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // RESTORE
    // =========================================================

    public async Task<bool> RestoreAsync(int id)
    {
        var transfer =
            await _context.StockTransfers
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted);

        if (transfer == null)
            return false;

        transfer.IsDeleted = false;

        transfer.UpdatedDate =
            DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }
}