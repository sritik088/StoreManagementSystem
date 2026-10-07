using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(
        ICustomerRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> CreateAsync(Customer customer)
    {
        var mobile =
            await _repository.GetByMobileAsync(customer.Mobile);

        if (mobile != null)
            throw new Exception("Mobile number already exists.");

        if (!string.IsNullOrWhiteSpace(customer.GSTNumber))
        {
            var gst =
                await _repository.GetByGSTAsync(customer.GSTNumber);

            if (gst != null)
                throw new Exception("GST number already exists.");
        }

        customer.CreatedDate = DateTime.Now;

        await _repository.AddAsync(customer);

        await _unitOfWork.CommitAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        var existing =
            await _repository.GetByIdAsync(customer.Id);

        if (existing == null)
            return false;

        existing.Name = customer.Name;
        existing.ContactPerson = customer.ContactPerson;
        existing.Mobile = customer.Mobile;
        existing.Email = customer.Email;
        existing.Address = customer.Address;
        existing.City = customer.City;
        existing.State = customer.State;
        existing.PostalCode = customer.PostalCode;
        existing.GSTNumber = customer.GSTNumber;
        existing.PANNumber = customer.PANNumber;
        existing.CreditLimit = customer.CreditLimit;
        existing.CreditDays = customer.CreditDays;
        existing.IsActive = customer.IsActive;
        existing.UpdatedDate = DateTime.Now;

        _repository.Update(existing);

        await _unitOfWork.CommitAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer =
            await _repository.GetByIdAsync(id);

        if (customer == null)
            return false;

        customer.IsDeleted = true;
        customer.UpdatedDate = DateTime.Now;

        _repository.Update(customer);

        await _unitOfWork.CommitAsync();

        return true;
    }
}
