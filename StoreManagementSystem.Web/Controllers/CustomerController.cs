using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Customer;

namespace StoreManagementSystem.Web.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ISystemSettingService _systemSettingService;

    public CustomerController(
        ICustomerService customerService,
        ISystemSettingService systemSettingService)
    {
        _customerService = customerService;
        _systemSettingService = systemSettingService;
    }

    // =========================================================
    // CUSTOMERS MODULE CHECK
    // =========================================================

    private async Task<bool> IsCustomersEnabledAsync()
    {
        return await _systemSettingService.GetCustomersEnabledAsync();
    }

    // =========================================================
    // INDEX
    // =========================================================

    public async Task<IActionResult> Index()
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        var customers = await _customerService.GetAllAsync();

        return View(customers);
    }

    // =========================================================
    // DETAILS
    // =========================================================

    public async Task<IActionResult> Details(int id)
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        var customer = await _customerService.GetByIdAsync(id);

        if (customer == null)
            return NotFound();

        return View(customer);
    }

    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        return View(new CustomerViewModel());
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerViewModel model)
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var customer = new Customer
        {
            Name = model.Name,
            ContactPerson = model.ContactPerson,
            Mobile = model.Mobile,
            Email = model.Email,
            Address = model.Address,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            GSTNumber = model.GSTNumber,
            PANNumber = model.PANNumber,
            CreditLimit = model.CreditLimit,
            CreditDays = model.CreditDays,
            IsActive = model.IsActive
        };

        await _customerService.CreateAsync(customer);

        TempData["Success"] = "Customer created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        var customer = await _customerService.GetByIdAsync(id);

        if (customer == null)
            return NotFound();

        var model = new CustomerViewModel
        {
            Id = customer.Id,
            Name = customer.Name,
            ContactPerson = customer.ContactPerson,
            Mobile = customer.Mobile,
            Email = customer.Email,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostalCode = customer.PostalCode,
            GSTNumber = customer.GSTNumber,
            PANNumber = customer.PANNumber,
            CreditLimit = customer.CreditLimit,
            CreditDays = customer.CreditDays,
            IsActive = customer.IsActive
        };

        return View(model);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerViewModel model)
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var customer = new Customer
        {
            Id = model.Id,
            Name = model.Name,
            ContactPerson = model.ContactPerson,
            Mobile = model.Mobile,
            Email = model.Email,
            Address = model.Address,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            GSTNumber = model.GSTNumber,
            PANNumber = model.PANNumber,
            CreditLimit = model.CreditLimit,
            CreditDays = model.CreditDays,
            IsActive = model.IsActive
        };

        var result = await _customerService.UpdateAsync(customer);

        if (!result)
            return NotFound();

        TempData["Success"] = "Customer updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // DELETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await IsCustomersEnabledAsync())
            return NotFound();

        var result = await _customerService.DeleteAsync(id);

        TempData[result ? "Success" : "Error"] =
            result
            ? "Customer deleted successfully."
            : "Customer not found.";

        return RedirectToAction(nameof(Index));
    }
}