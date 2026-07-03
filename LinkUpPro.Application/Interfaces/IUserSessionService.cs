using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IUserSessionService
    {
        Task SignInAsync(ApplicationUser user, bool rememberMe);

        Task SignOutAsync();
    }
}
