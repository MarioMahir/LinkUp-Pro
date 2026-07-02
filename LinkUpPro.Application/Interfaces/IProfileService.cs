using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.ViewModels;

namespace LinkUpPro.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(string userId);

        Task<ServiceResult> UpdateProfileAsync(ProfileViewModel vm, string userId);
    }
}
