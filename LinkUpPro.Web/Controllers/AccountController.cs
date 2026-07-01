using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(
            IAccountService accountService)
        {
            _accountService = accountService;
        }

        // LOGIN

        [HttpGet]
        public IActionResult Login(string message)
        {
            if (message == "inactivity")
            {
                TempData["Error"] = "Su sesión finalizó por inactividad. Inicie sesión nuevamente.";
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result =
                await _accountService
                .LoginAsync(vm);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    "",
                    result.Message);

                return View(vm);
            }

            return RedirectToAction(
                "Index",
                "Home");
        }

        // REGISTER

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result =
                await _accountService
                .RegisterAsync(vm);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    "",
                    result.Message);

                return View(vm);
            }

            TempData["Message"] =
            "Cuenta creada correctamente. Revise su correo.";

            return RedirectToAction(
                "Login");
        }

        // ACTIVAR CUENTA

        [HttpGet]
        public async Task<IActionResult>
        ActivateAccount(
            string userId,
            string token)
        {
            if (string.IsNullOrEmpty(userId)
                || string.IsNullOrEmpty(token))
            {
                TempData["Error"] =
                "Enlace inválido.";

                return RedirectToAction(
                    "Login");
            }

            var result =
                await _accountService
                .ActivateAccountAsync(
                    userId,
                    token);

            if (!result.Succeeded)
            {
                TempData["Error"] =
                "El enlace de activación no es válido o ya fue utilizado.";

                return RedirectToAction(
                    "Login");
            }

            TempData["Message"] =
            "Su cuenta fue activada correctamente.";

            return RedirectToAction(
                "Login");
        }

        // FORGOT PASSWORD

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        ForgotPassword(
            ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result =
                await _accountService
                .ForgotPasswordAsync(vm);

            TempData["Message"] =
                result.Message;

            return RedirectToAction(
                "Login");
        }

        // RESET PASSWORD

        [HttpGet]
        public IActionResult
        ResetPassword(
            string userId,
            string token)
        {
            if (string.IsNullOrEmpty(userId)
                || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "Login");
            }

            return View(
                new ResetPasswordViewModel
                {
                    UserId = userId,
                    Token = token
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        ResetPassword(
            ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result =
                await _accountService
                .ResetPasswordAsync(vm);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    "",
                    result.Message);

                return View(vm);
            }

            TempData["Message"] =
            "Su contraseña fue restablecida correctamente.";

            return RedirectToAction(
                "Login");
        }

        // LOGOUT

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        Logout()
        {
            await _accountService
                .LogoutAsync();

            return RedirectToAction(
                "Login");
        }

        [HttpGet]
        public IActionResult
ResendActivation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult>
        ResendActivation(
        ResendActivationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result =
            await _accountService
            .ResendActivationAsync(vm);

            TempData["Message"] =
            result.Message;

            return RedirectToAction(
            "Login");
        }
    }
}