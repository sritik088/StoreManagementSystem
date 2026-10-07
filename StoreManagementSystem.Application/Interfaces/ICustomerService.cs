using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();

    Task<Customer?> GetByIdAsync(int id);

    Task<bool> CreateAsync(Customer customer);

    Task<bool> UpdateAsync(Customer customer);

    Task<bool> DeleteAsync(int id);
}
