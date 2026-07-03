using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace LinkUpPro.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IFileStorageService _fileStorageService;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private static readonly Regex PhoneRegex = new(@"^(809|829|849)-\d{3}-\d{4}$");

        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileStorageService fileStorageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileStorageService = fileStorageService;
        }

        private static ServiceResult Fail(string message) =>
            new() { Succeeded = false, Message = message };

        public async Task<ProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Email = user.Email,
                UserName = user.UserName,
                CurrentProfilePictureUrl = user.ProfilePictureUrl
            };
        }

        public async Task<ServiceResult> UpdateProfileAsync(ProfileViewModel vm, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Fail("No posee permisos para modificar este perfil.");
            }

            if (string.IsNullOrWhiteSpace(vm.FirstName))
            {
                return Fail("Debe ingresar su nombre.");
            }

            if (string.IsNullOrWhiteSpace(vm.LastName))
            {
                return Fail("Debe ingresar su apellido.");
            }

            if (string.IsNullOrWhiteSpace(vm.PhoneNumber) || !PhoneRegex.IsMatch(vm.PhoneNumber.Trim()))
            {
                return Fail("Debe ingresar un número telefónico válido de República Dominicana.");
            }

            var wantsPasswordChange = !string.IsNullOrEmpty(vm.CurrentPassword)
                || !string.IsNullOrEmpty(vm.NewPassword)
                || !string.IsNullOrEmpty(vm.ConfirmPassword);

            if (wantsPasswordChange)
            {
                if (string.IsNullOrEmpty(vm.CurrentPassword) ||
                    string.IsNullOrEmpty(vm.NewPassword) ||
                    string.IsNullOrEmpty(vm.ConfirmPassword))
                {
                    return Fail("Para cambiar su contraseña debe completar la contraseña actual, la nueva contraseña y su confirmación.");
                }

                if (!await _userManager.CheckPasswordAsync(user, vm.CurrentPassword))
                {
                    return Fail("La contraseña actual es incorrecta.");
                }

                if (vm.NewPassword != vm.ConfirmPassword)
                {
                    return Fail("La nueva contraseña y su confirmación no coinciden.");
                }

                if (!PasswordRegex.IsMatch(vm.NewPassword))
                {
                    return Fail("La nueva contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                }

                if (vm.NewPassword == vm.CurrentPassword)
                {
                    return Fail("La nueva contraseña debe ser diferente de la contraseña actual.");
                }
            }

            string? newImagePath = null;

            if (vm.ProfilePictureFile != null)
            {
                var extension = Path.GetExtension(vm.ProfilePictureFile.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    return Fail("El archivo seleccionado no tiene un formato de imagen válido.");
                }

                if (vm.ProfilePictureFile.Length > 5242880)
                {
                    return Fail("La imagen seleccionada no puede superar los 5 MB.");
                }

                if (!FileSignatureValidator.HasValidImageSignature(vm.ProfilePictureFile))
                {
                    return Fail("El archivo seleccionado no tiene un formato de imagen válido.");
                }

                newImagePath = await _fileStorageService.SaveAsync(vm.ProfilePictureFile, "users");
            }

            var previousImage = user.ProfilePictureUrl;

            user.FirstName = vm.FirstName.Trim();
            user.LastName = vm.LastName.Trim();
            user.PhoneNumber = vm.PhoneNumber.Trim();
            user.UpdatedDate = DateTime.UtcNow;

            if (newImagePath != null)
            {
                user.ProfilePictureUrl = newImagePath;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Fail(updateResult.Errors.First().Description);
            }

            if (newImagePath != null)
            {
                _fileStorageService.DeleteIfExists(previousImage);
            }

            if (wantsPasswordChange)
            {
                var changeResult = await _userManager.ChangePasswordAsync(
                    user, vm.CurrentPassword!, vm.NewPassword!);

                if (!changeResult.Succeeded)
                {
                    return Fail(changeResult.Errors.First().Description);
                }

                await _userManager.UpdateSecurityStampAsync(user);
                await _signInManager.SignOutAsync();

                return new ServiceResult
                {
                    Succeeded = true,
                    Message = "Su perfil y contraseña fueron actualizados correctamente. Inicie sesión nuevamente.",
                    RequiresReLogin = true
                };
            }

            return new ServiceResult
            {
                Succeeded = true,
                Message = "Su perfil fue actualizado correctamente."
            };
        }
    }
}
