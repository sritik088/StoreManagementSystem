using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class DeliveryRepository
        : IDeliveryRepository
    {
        private readonly ApplicationDbContext _context;

        public DeliveryRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<List<Delivery>> GetAllAsync()
        {
            return await _context.Deliveries
                .Include(x => x.SalesOrder)
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.DeliveryDate)
                .ThenByDescending(x => x.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<Delivery?> GetByIdAsync(int id)
        {
            return await _context.Deliveries
                .Include(x => x.SalesOrder)
                    .ThenInclude(x => x.SalesOrderItems)
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Items)
                    .ThenInclude(x => x.SalesOrderItem)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // =====================================================
        // GET BY SALES ORDER
        // =====================================================

        public async Task<List<Delivery>>
            GetBySalesOrderIdAsync(int salesOrderId)
        {
            return await _context.Deliveries
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .Where(x =>
                    x.SalesOrderId == salesOrderId)
                .OrderByDescending(x => x.DeliveryDate)
                .ToListAsync();
        }

        // =====================================================
        // GET DELIVERED QUANTITY
        // =====================================================

        public async Task<decimal>
            GetDeliveredQuantityAsync(
                int salesOrderItemId)
        {
            return await _context.DeliveryItems
                .Where(x =>
                    x.SalesOrderItemId ==
                    salesOrderItemId)
                .SumAsync(x => x.Quantity);
        }

        // =====================================================
        // ADD
        // =====================================================

        public async Task AddAsync(
            Delivery delivery)
        {
            await _context.Deliveries
                .AddAsync(delivery);
        }
    }
}