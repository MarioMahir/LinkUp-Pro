using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResult> LoginAsync(LoginViewModel vm);

        Task<ServiceResult> RegisterAsync(RegisterViewModel vm);

        Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordViewModel vm);

        Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel vm);

        Task<ServiceResult> ActivateAccountAsync(string userId, string token);

        Task<ServiceResult> ResendActivationAsync(ResendActivationViewModel vm);

        Task LogoutAsync();
    }
}
