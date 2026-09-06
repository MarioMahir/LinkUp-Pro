using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUserSessionService _userSessionService;

        private readonly IEmailService _emailService;

        private readonly IFileStorageService _fileStorageService;

        private readonly ILinkBuilderService _linkBuilderService;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            IUserSessionService userSessionService,
            IEmailService emailService,
            IFileStorageService fileStorageService,
            ILinkBuilderService linkBuilderService)
        {
            _userManager = userManager;
            _userSessionService = userSessionService;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
            _linkBuilderService = linkBuilderService;
        }

        private const string GenericLoginError =
            "El nombre de usuario o la contraseña son incorrectos.";

        private const string LockedOutError =
            "La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Inténtelo nuevamente en 15 minutos o restablezca su contraseña.";

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
                    Message = GenericLoginError
                };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return new()
                {
                    Succeeded = false,
                    Message = LockedOutError
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
                        Message = LockedOutError
                    };
                }

                return new()
                {
                    Succeeded = false,
                    Message = GenericLoginError
                };
            }

            // La contraseña es correcta: informar del estado inactivo aquí no
            // permite enumerar usuarios registrados.
            if (!user.EmailConfirmed)
            {
                return new()
                {
                    Succeeded = false,
                    Message =
                    "Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico."
                };
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            await _userSessionService.SignInAsync(
                user,
                vm.RememberMe);

            return new()
            {
                Succeeded = true
            };
        }

        public async Task<ServiceResult> RegisterAsync(RegisterViewModel vm)
        {
            // Unicidad sin distinguir mayúsculas (Identity normaliza a mayúsculas)
            if (await _userManager.FindByNameAsync(vm.UserName.Trim()) != null)
            {
                return new() { Succeeded = false, Message = "Este nombre de usuario ya se encuentra registrado." };
            }

            if (await _userManager.FindByEmailAsync(vm.Email.Trim()) != null)
            {
                return new() { Succeeded = false, Message = "Este correo electrónico ya se encuentra registrado." };
            }

            string imagePath = await _fileStorageService.SaveAsync(vm.ProfilePicture, "users");

            var user = new ApplicationUser
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                UserName = vm.UserName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                ProfilePictureUrl = imagePath,
                LastActivationRequestDate = DateTime.UtcNow
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

            var link =
                _linkBuilderService.BuildAbsoluteUrl(
                    $"/Account/ActivateAccount?userId={user.Id}&token={Uri.EscapeDataString(token)}");

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

                var link =
                    _linkBuilderService.BuildAbsoluteUrl(
                        $"/Account/ResetPassword?userId={user.Id}&token={Uri.EscapeDataString(token)}");

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
            await _userSessionService
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

            var link =
            _linkBuilderService.BuildAbsoluteUrl(
            $"/Account/ActivateAccount?userId={user.Id}&token={Uri.EscapeDataString(token)}");

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
