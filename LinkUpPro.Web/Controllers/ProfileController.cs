using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUpPro.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(
            IProfileService profileService)
        {
            _profileService = profileService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await _profileService.GetProfileAsync(CurrentUserId);

            if (vm == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var currentForInvalidModel = await _profileService.GetProfileAsync(CurrentUserId);
                vm.CurrentProfilePictureUrl = currentForInvalidModel?.CurrentProfilePictureUrl;
                vm.Email = currentForInvalidModel?.Email;
                vm.UserName = currentForInvalidModel?.UserName;
                return View(vm);
            }

            var result = await _profileService.UpdateProfileAsync(vm, CurrentUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Message);
                var current = await _profileService.GetProfileAsync(CurrentUserId);
                vm.CurrentProfilePictureUrl = current?.CurrentProfilePictureUrl;
                vm.Email = current?.Email;
                vm.UserName = current?.UserName;
                return View(vm);
            }

            if (result.RequiresReLogin)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Login", "Account");
            }

            TempData["Message"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}
