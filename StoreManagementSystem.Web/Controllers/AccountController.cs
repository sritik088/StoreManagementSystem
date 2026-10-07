
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Web.ViewModels.Account;


namespace StoreManagementSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // =========================================================
    // LOGIN - GET
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }

    // =========================================================
    // LOGIN - POST
    // =========================================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // -----------------------------------------------------
        // FIND USER BY EMAIL
        // -----------------------------------------------------

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View(model);
        }

        // -----------------------------------------------------
        // CHECK ACTIVE STATUS
        // -----------------------------------------------------

        if (!user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is inactive. Please contact the administrator.");

            return View(model);
        }

        // -----------------------------------------------------
        // LOGIN
        // -----------------------------------------------------

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        // -----------------------------------------------------
        // SUCCESS
        // -----------------------------------------------------

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        // -----------------------------------------------------
        // LOCKED OUT
        // -----------------------------------------------------

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account has been locked due to multiple failed login attempts.");

            return View(model);
        }

        // -----------------------------------------------------
        // NOT ALLOWED
        // -----------------------------------------------------

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(
                string.Empty,
                "You are not allowed to log in. Please contact the administrator.");

            return View(model);
        }

        // -----------------------------------------------------
        // FAILED
        // -----------------------------------------------------

        ModelState.AddModelError(
            string.Empty,
            "Invalid email or password.");

        return View(model);
    }

    // =========================================================
    // REGISTER - GET
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // =========================================================
    // REGISTER - POST
    // =========================================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // -----------------------------------------------------
        // CHECK EXISTING EMAIL
        // -----------------------------------------------------

        var existingUser =
            await _userManager.FindByEmailAsync(model.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                "Email",
                "An account with this email already exists.");

            return View(model);
        }

        // -----------------------------------------------------
        // CREATE USER
        // -----------------------------------------------------

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,

            FirstName = model.FirstName,
            LastName = model.LastName,

            PhoneNumber = model.PhoneNumber,

            EmailConfirmed = true,

            IsActive = true,

            CreatedDate = DateTime.Now,

            // Public registration does not assign a warehouse
            WarehouseId = null
        };

        var result = await _userManager.CreateAsync(
            user,
            model.Password);

        if (result.Succeeded)
        {
            // -------------------------------------------------
            // DEFAULT ROLE
            // -------------------------------------------------

            await _userManager.AddToRoleAsync(
                user,
                RoleConstants.Cashier);

            // -------------------------------------------------
            // LOGIN AFTER REGISTRATION
            // -------------------------------------------------

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        // -----------------------------------------------------
        // IDENTITY ERRORS
        // -----------------------------------------------------

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }

        return View(model);
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }

    // =========================================================
    // ACCESS DENIED
    // =========================================================

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // =========================================================
    // FORGOT PASSWORD - GET
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // =========================================================
    // FORGOT PASSWORD - POST
    // =========================================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user =
            await _userManager.FindByEmailAsync(model.Email);

        // -----------------------------------------------------
        // SECURITY
        // -----------------------------------------------------
        // Do not reveal whether an email exists.

        if (user == null || !user.EmailConfirmed)
        {
            return View("ForgotPasswordConfirmation");
        }

        // -----------------------------------------------------
        // GENERATE RESET TOKEN
        // -----------------------------------------------------

        var token =
            await _userManager.GeneratePasswordResetTokenAsync(user);

        // -----------------------------------------------------
        // DEVELOPMENT RESET URL
        // -----------------------------------------------------
        // Replace this with your email service when email
        // sending is implemented.

        var resetUrl = Url.Action(
            nameof(ResetPassword),
            "Account",
            new
            {
                token,
                email = user.Email
            },
            Request.Scheme);

        TempData["ResetPasswordUrl"] = resetUrl;

        return View("ForgotPasswordConfirmation");
    }

    // =========================================================
    // RESET PASSWORD - GET
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(
        string? token,
        string? email)
    {
        if (string.IsNullOrEmpty(token) ||
            string.IsNullOrEmpty(email))
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    // =========================================================
    // RESET PASSWORD - POST
    // =========================================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user =
            await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            // Do not reveal whether the account exists.
            return RedirectToAction(nameof(Login));
        }

        var result =
            await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

        if (result.Succeeded)
        {
            TempData["Success"] =
                "Your password has been reset successfully.";

            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }

        return View(model);
    }
}
