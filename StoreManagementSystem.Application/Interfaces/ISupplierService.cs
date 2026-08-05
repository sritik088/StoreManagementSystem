using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync();

        Task<Supplier?> GetByIdAsync(int id);

        Task<IEnumerable<Supplier>> SearchAsync(string search);

        Task<bool> CreateAsync(Supplier supplier);

        Task<bool> UpdateAsync(Supplier supplier);

        Task<bool> DeleteAsync(int id);
    }
}
