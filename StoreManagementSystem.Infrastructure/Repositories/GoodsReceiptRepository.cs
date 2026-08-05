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

        public GoodsReceiptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GoodsReceipt>> GetAllAsync()
        {
            return await _context.GoodsReceipts
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .AsNoTracking()
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync();
        }

        public async Task<GoodsReceipt?> GetByIdAsync(int id)
        {
            return await _context.GoodsReceipts
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<string> GenerateGRNNumberAsync()
        {
            string prefix = $"GRN-{DateTime.Now:yyyyMMdd}-";

            var last = await _context.GoodsReceipts
                .Where(x => x.GRNNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            int next = 1;

            if (last != null)
            {
                var number = last.GRNNumber.Substring(prefix.Length);

                if (int.TryParse(number, out int n))
                    next = n + 1;
            }

            return $"{prefix}{next:0000}";
        }

        public async Task<IEnumerable<PurchaseOrder>> GetApprovedPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders

                .Include(x => x.Supplier)

                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)

                .Where(x =>
                    x.Status == PurchaseOrderStatus.Approved &&
                    !x.IsDeleted)

                .OrderByDescending(x => x.OrderDate)

                .ToListAsync();
        }

        public async Task AddAsync(GoodsReceipt receipt)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.GoodsReceipts.AddAsync(receipt);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        public async Task UpdateAsync(GoodsReceipt receipt)
        {
            _context.GoodsReceipts.Update(receipt);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var receipt = await _context.GoodsReceipts.FindAsync(id);

            if (receipt == null)
                return;

            receipt.IsDeleted = true;

            receipt.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}