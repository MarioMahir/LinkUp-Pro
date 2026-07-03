using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IEmailService _emailService;

        private readonly IFileStorageService _fileStorageService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IFileStorageService fileStorageService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResult>
LoginAsync(LoginViewModel vm)
        {
            var user =
                await _userManager
                .FindByNameAsync(vm.UserName);

            if (user == null)
            {
                return new()
                {
                    Succeeded = false,
                    Message =
                    "El nombre de usuario o la contraseña son incorrectos."
                };
            }

            if (!user.EmailConfirmed)
            {
                return new()
                {
                    Succeeded = false,
                    Message =
                    "Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico."
                };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return new()
                {
                    Succeeded = false,
                    Message =
                    "La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Inténtelo nuevamente en 15 minutos o restablezca su contraseña."
                };
            }

            var passwordCorrect =
                await _userManager.CheckPasswordAsync(
                    user,
                    vm.Password);

            if (!passwordCorrect)
            {
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return new()
                    {
                        Succeeded = false,
                        Message =
                        "La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Inténtelo nuevamente en 15 minutos o restablezca su contraseña."
                    };
                }

                return new()
                {
                    Succeeded = false,
                    Message =
                    "El nombre de usuario o la contraseña son incorrectos."
                };
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                ExpiresUtc = vm.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };

            await _signInManager.SignInAsync(
                user,
                authProperties);

            return new()
            {
                Succeeded = true
            };
        }

        public async Task<ServiceResult> RegisterAsync(RegisterViewModel vm)
        {
            string imagePath = await _fileStorageService.SaveAsync(vm.ProfilePicture, "users");

            var user = new ApplicationUser
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                UserName = vm.UserName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                ProfilePictureUrl = imagePath
            };

            var result =
                await _userManager
                .CreateAsync(
                    user,
                    vm.Password);

            if (!result.Succeeded)
            {
                return new()
                {
                    Succeeded = false,
                    Message =
                    result.Errors.First().Description
                };
            }

            var token =
                await _userManager
                .GenerateEmailConfirmationTokenAsync(
                    user);

            var request =
                _httpContextAccessor
                .HttpContext
                .Request;

            var baseUrl =
                $"{request.Scheme}://{request.Host}";

            var link =
                $"{baseUrl}/Account/ActivateAccount?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Activación de cuenta",

$@"
<h2>Bienvenido a LinkUp Pro</h2>

<p>
Tu cuenta fue creada correctamente.
</p>

<p>
Haz clic para activarla:
</p>

<a href='{link}'
style='padding:10px;
background:#0d6efd;
color:white;
text-decoration:none;
border-radius:5px;'>

Activar cuenta

</a>
");

            return new()
            {
                Succeeded = true,
                Message =
                "Cuenta creada correctamente."
            };
        }

        public async Task<ServiceResult>
        ActivateAccountAsync(
            string userId,
            string token)
        {
            var user =
                await _userManager
                .FindByIdAsync(userId);

            if (user == null)
            {
                return new()
                {
                    Succeeded = false
                };
            }

            var result =
                await _userManager
                .ConfirmEmailAsync(
                    user,
                    token);

            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            return new()
            {
                Succeeded = result.Succeeded
            };
        }

        public async Task<ServiceResult>
        ForgotPasswordAsync(
            ForgotPasswordViewModel vm)
        {
            var user =
                await _userManager
                .FindByNameAsync(
                    vm.UserName);

            if (user != null)
            {
                var token =
                    await _userManager
                    .GeneratePasswordResetTokenAsync(
                        user);

                var request =
                    _httpContextAccessor
                    .HttpContext
                    .Request;

                var baseUrl =
                    $"{request.Scheme}://{request.Host}";

                var link =
$"{baseUrl}/Account/ResetPassword?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService
                    .SendEmailAsync(
                    user.Email,
                    "Restablecimiento de contraseña",

$@"
<h2>LinkUp Pro</h2>

<p>
Has solicitado restablecer tu contraseña.
</p>

<a href='{link}'
style='padding:10px;
background:#0d6efd;
color:white;
text-decoration:none;
border-radius:5px;'>

Restablecer contraseña

</a>");
            }

            return new()
            {
                Succeeded = true,
                Message =
                "Si el nombre de usuario corresponde a una cuenta registrada, recibirá un enlace para restablecer su contraseña."
            };
        }

        public async Task<ServiceResult>
        ResetPasswordAsync(
            ResetPasswordViewModel vm)
        {
            var user =
                await _userManager
                .FindByIdAsync(vm.UserId);

            if (user == null)
            {
                return new()
                {
                    Succeeded = false
                };
            }

            var result =
                await _userManager
                .ResetPasswordAsync(
                    user,
                    vm.Token,
                    vm.NewPassword);

            if (!result.Succeeded)
            {
                return new()
                {
                    Succeeded = false,
                    Message =
                    result.Errors.First().Description
                };
            }

            await _userManager
                .ResetAccessFailedCountAsync(user);

            await _userManager
                .SetLockoutEndDateAsync(user, null);

            await _userManager.UpdateSecurityStampAsync(user);

            return new()
            {
                Succeeded = true
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager
                .SignOutAsync();
        }

        public async Task<ServiceResult>
ResendActivationAsync(
ResendActivationViewModel vm)
        {
            var user =
            await _userManager
            .FindByNameAsync(
            vm.UserName);

            if (user == null
                || user.EmailConfirmed)
            {
                return new()
                {
                    Succeeded = true,

                    Message =
                    "Si la cuenta existe y todavía no ha sido activada, recibirá un nuevo enlace."
                };
            }

            if (user.LastActivationRequestDate
                >= DateTime.UtcNow.AddMinutes(-5))
            {
                return new()
                {
                    Succeeded = true,

                    Message =
                    "Espere 5 minutos antes de solicitar otro enlace."
                };
            }

            var token =
            await _userManager
            .GenerateEmailConfirmationTokenAsync(
            user);

            var request =
            _httpContextAccessor.HttpContext.Request;

            var baseUrl =
            $"{request.Scheme}://{request.Host}";

            var link =
            $"{baseUrl}/Account/ActivateAccount?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailService
            .SendEmailAsync(
            user.Email,
            "Nuevo enlace",

            $"<a href='{link}'>Activar cuenta</a>");

            user.LastActivationRequestDate =
            DateTime.UtcNow;

            await _userManager
            .UpdateAsync(user);

            return new()
            {
                Succeeded = true,

                Message =
                "Si la cuenta existe y todavía no ha sido activada, recibirá un nuevo enlace."
            };
        }
    }
}