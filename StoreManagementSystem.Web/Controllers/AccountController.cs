
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Web.ViewModels.Account;

namespace StoreManagementSystem.Web.Controllers;

[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None)]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;

    private const string GenericLoginError =
        "Invalid email or password.";

    private const string GenericForgotPasswordMessage =
        "If an eligible account exists for that email address, " +
        "a password reset link will be sent.";

    public AccountController(
       UserManager<ApplicationUser> userManager,
       SignInManager<ApplicationUser> signInManager,
       IWebHostEnvironment environment,
       IEmailService emailService,
       ILogger<AccountController> logger,
       IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    // =========================================================
    // LOGIN - GET
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = "/Account/Login")
    {
        //if (User.Identity?.IsAuthenticated == true)
        //{
        //    return RedirectToAction("Index", "Dashboard");
        //}
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

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Keep the public error consistent for unknown/inactive users.
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                GenericLoginError);

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError(
            string.Empty,
            GenericLoginError);

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

        // Avoid disclosing account existence.
        var existingUser =
            await _userManager.FindByEmailAsync(model.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Registration could not be completed. " +
                "Check your details or contact an administrator.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,

            // Prefer a proper email-confirmation workflow for production.
            EmailConfirmed = true,

            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            WarehouseId = null
        };

        var result = await _userManager.CreateAsync(
            user,
            model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                // Identity errors can reveal policy/configuration details.
                // Return a generic error to the public registration form.
                _logger.LogWarning(
                    "Registration failed with Identity error code {Code}.",
                    error.Code);
            }

            ModelState.AddModelError(
                string.Empty,
                "Registration could not be completed. " +
                "Please check your details and try again.");

            return View(model);
        }

        // Never accept a role from user-submitted form data.
        var roleResult = await _userManager.AddToRoleAsync(
            user,
            RoleConstants.Cashier);

        if (!roleResult.Succeeded)
        {
            _logger.LogError(
                "Failed to assign the default registration role " +
                "to newly created user {UserId}.",
                user.Id);

            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                "Registration could not be completed. Please try again.");

            return View(model);
        }

        await _signInManager.SignInAsync(
            user,
            isPersistent: false);

        return RedirectToAction("Index", "Dashboard");
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            var stampResult =
                await _userManager.UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
            {
                _logger.LogError(
                    "Failed to update security stamp for user {UserId}.",
                    user.Id);
            }
        }

        await _signInManager.SignOutAsync();

        Response.Headers["Cache-Control"] =
            "no-store, no-cache, must-revalidate, private";

        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return RedirectToAction(nameof(Login));
    }

    // =========================================================
    // ACCESS DENIED
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
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

        try
        {
            var user = await _userManager.FindByEmailAsync(
                model.Email.Trim());

            if (user != null && user.EmailConfirmed)
            {
                var token =
                    await _userManager.GeneratePasswordResetTokenAsync(user);

                // Local development can use the current application URL.
                // Production must use an explicitly configured HTTPS URL.
                string? baseUrl;

                if (_environment.IsDevelopment())
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                }
                else
                {
                    baseUrl = _configuration["AppUrls:PublicBaseUrl"];
                }

                if (!Uri.TryCreate(
                        baseUrl,
                        UriKind.Absolute,
                        out var baseUri) ||
                    (!_environment.IsDevelopment() &&
                     baseUri.Scheme != Uri.UriSchemeHttps))
                {
                    _logger.LogError(
                        "Password reset email skipped because " +
                        "AppUrls:PublicBaseUrl is missing or invalid.");
                }
                else
                {
                    var resetUrl = Url.Action(
                        nameof(ResetPassword),
                        "Account",
                        new
                        {
                            token,
                            email = user.Email
                        },
                        protocol: baseUri.Scheme,
                        host: baseUri.Authority);

                    if (string.IsNullOrWhiteSpace(resetUrl))
                    {
                        _logger.LogError(
                            "Password reset URL generation failed.");
                    }
                    else
                    {
                        var safeResetUrl =
                            HtmlEncoder.Default.Encode(resetUrl);

                        var htmlBody = $"""
                        <!DOCTYPE html>
                        <html lang="en">
                        <head>
                            <meta charset="utf-8">
                            <meta name="viewport"
                                  content="width=device-width, initial-scale=1">
                            <title>Password Reset</title>
                        </head>
                        <body style="font-family:Arial,sans-serif;
                                     background:#f4f6f9;padding:24px;color:#222">
                            <div style="max-width:560px;margin:auto;
                                        background:#fff;padding:28px;
                                        border-radius:10px">
                                <h2>Reset your password</h2>
                                <p>
                                    We received a request to reset your
                                    Store Management System password.
                                </p>
                                <p>
                                    Select the button below to choose a new password.
                                </p>
                                <p style="margin:28px 0">
                                    <a href="{safeResetUrl}"
                                       style="display:inline-block;
                                              padding:12px 22px;
                                              background:#2563eb;color:#fff;
                                              text-decoration:none;
                                              border-radius:6px">
                                        Reset Password
                                    </a>
                                </p>
                                <p>
                                    If you did not request this change,
                                    you can ignore this email.
                                </p>
                                <p style="font-size:12px;color:#666">
                                    Do not share this link with anyone.
                                </p>
                            </div>
                        </body>
                        </html>
                        """;

                        await _emailService.SendEmailAsync(
                            user.Email!,
                            "Reset your Store Management System password",
                            htmlBody,
                            HttpContext.RequestAborted);

                        _logger.LogInformation(
                            "Password reset email accepted by SMTP " +
                            "for an eligible account.");
                    }
                }
            }
        }
        catch (OperationCanceledException)
            when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Password reset email processing failed.");
        }

        ViewData["ConfirmationMessage"] =
            GenericForgotPasswordMessage;

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
        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] =
                "The password reset link is invalid or incomplete.";

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

        if (string.IsNullOrWhiteSpace(model.Token) ||
            string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(
                string.Empty,
                "The password reset link is invalid or expired.");

            return View(model);
        }

        var user =
            await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The password reset link is invalid or expired.");

            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            model.Token,
            model.Password);

        if (result.Succeeded)
        {
            TempData["Success"] =
                "Your password has been reset successfully. Please log in.";

            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(
            string.Empty,
            "The password could not be reset. The link may have expired. " +
            "Please request a new reset link.");

        return View(model);
    }
}
