
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using StoreManagementSystem.Infrastructure.Identity;

namespace StoreManagementSystem.Web.Authorization
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermissionPolicyProvider(
            IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider =
                new DefaultAuthorizationPolicyProvider(options);
        }

        // =========================================================
        // DEFAULT POLICY
        // =========================================================

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        // =========================================================
        // FALLBACK POLICY
        // =========================================================

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        // =========================================================
        // PERMISSION POLICY
        // =========================================================

        public Task<AuthorizationPolicy?> GetPolicyAsync(
            string policyName)
        {
            // -----------------------------------------------------
            // Check whether this is one of our permission policies
            // -----------------------------------------------------

            if (PermissionConstants.All.ContainsKey(policyName))
            {
                var policy =
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                            // -------------------------------------------------
                            // SUPER ADMIN BYPASS
                            // -------------------------------------------------
                            // SuperAdmin automatically has access to every
                            // permission-protected page.
                            // -------------------------------------------------

                            context.User.IsInRole(
                                RoleConstants.SuperAdmin)

                            ||

                            // -------------------------------------------------
                            // NORMAL USER PERMISSION CHECK
                            // -------------------------------------------------
                            // Other users must have the required permission
                            // claim.
                            // -------------------------------------------------

                            context.User.HasClaim(
                                PermissionClaim.Type,
                                policyName)
                        )
                        .Build();

                return Task.FromResult<AuthorizationPolicy?>(
                    policy);
            }

            // -----------------------------------------------------
            // Not a permission policy
            // Use normal ASP.NET Core authorization.
            // -----------------------------------------------------

            return _fallbackPolicyProvider
                .GetPolicyAsync(policyName);
        }
    }
}

