using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            return await _context.PurchaseOrders
                .Include(x => x.Supplier)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(x => x.Supplier)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<string> GeneratePONumberAsync()
        {
            string prefix = $"PO-{DateTime.Now:yyyyMMdd}-";

            var lastPo = await _context.PurchaseOrders
                .Where(x => x.PONumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            int next = 1;

            if (lastPo != null)
            {
                string last = lastPo.PONumber.Substring(prefix.Length);

                if (int.TryParse(last, out int number))
                    next = number + 1;
            }

            return $"{prefix}{next:0000}";
        }

        public async Task AddAsync(PurchaseOrder purchaseOrder)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.PurchaseOrders.AddAsync(purchaseOrder);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task UpdateAsync(PurchaseOrder purchaseOrder)
        {
            _context.PurchaseOrders.Update(purchaseOrder);
            return Task.CompletedTask;

        }

        public async Task DeleteAsync(int id)
        {
            var po = await _context.PurchaseOrders.FindAsync(id);

            if (po == null)
                return;

            po.IsDeleted = true;
            po.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            var po = await _context.PurchaseOrders.FindAsync(id);

            if (po == null)
                return;

            po.Status = PurchaseOrderStatus.Approved;

            po.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(int id)
        {
            var po = await _context.PurchaseOrders.FindAsync(id);

            if (po == null)
                return;

            po.Status = PurchaseOrderStatus.Cancelled;

            po.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
