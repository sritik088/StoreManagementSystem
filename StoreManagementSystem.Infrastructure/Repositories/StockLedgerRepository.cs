using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class StockLedgerRepository : IStockLedgerRepository
    {
        private readonly ApplicationDbContext _context;

        public StockLedgerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StockLedger ledger)
        {
            await _context.StockLedgers.AddAsync(ledger);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockLedger>> GetByProductAsync(int productId)
        {
            return await _context.StockLedgers
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }
    }
}
