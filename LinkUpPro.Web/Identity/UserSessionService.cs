using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Web.Identity
{
    public class UserSessionService : IUserSessionService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserSessionService(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task SignInAsync(ApplicationUser user, bool rememberMe)
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };

            await _signInManager.SignInAsync(user, authProperties);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
