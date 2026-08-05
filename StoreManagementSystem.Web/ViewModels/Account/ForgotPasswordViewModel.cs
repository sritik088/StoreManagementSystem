using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}