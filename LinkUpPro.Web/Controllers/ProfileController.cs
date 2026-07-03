using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            IProfileService profileService,
            UserManager<ApplicationUser> userManager)
        {
            _profileService = profileService;
            _userManager = userManager;
        }

        private string CurrentUserId => _userManager.GetUserId(User)!;

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
