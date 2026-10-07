using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);
    }

    public async Task<Customer?> GetByMobileAsync(string mobile)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.Mobile == mobile &&
                !x.IsDeleted);
    }

    public async Task<Customer?> GetByGSTAsync(string gst)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(x =>
                x.GSTNumber == gst &&
                !x.IsDeleted);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers
            .AnyAsync(x => x.Id == id);
    }
}
