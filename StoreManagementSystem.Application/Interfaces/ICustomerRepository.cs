using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();

    Task<Customer?> GetByIdAsync(int id);

    Task<Customer?> GetByMobileAsync(string mobile);

    Task<Customer?> GetByGSTAsync(string gst);

    Task AddAsync(Customer customer);

    void Update(Customer customer);

    Task<bool> ExistsAsync(int id);
}
