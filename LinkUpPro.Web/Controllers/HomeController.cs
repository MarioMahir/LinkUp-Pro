using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPostService
            _postService;

        private readonly UserManager<ApplicationUser>
            _userManager;

        public HomeController(
            IPostService postService,
            UserManager<ApplicationUser>
            userManager)
        {
            _postService =
                postService;

            _userManager =
                userManager;
        }

        [HttpGet]
        public async Task<IActionResult>
        Index()
        {
            var user =
                await _userManager
                .GetUserAsync(User);

            HomeViewModel vm =
                new();

            vm.Posts =
                await _postService
                .GetAllByUserAsync(
                    user.Id);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        CreatePost(
            HomeViewModel vm)
        {
            try
            {
                var user =
                    await _userManager
                    .GetUserAsync(User);

                await _postService
                    .CreateAsync(
                        vm.NewPost,
                        user.Id);

                TempData["Message"] =
                "La publicación fue creada correctamente.";

                return RedirectToAction(
                    "Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                var user =
                    await _userManager
                    .GetUserAsync(User);

                vm.Posts =
                    await _postService
                    .GetAllByUserAsync(
                        user.Id);

                return View(
                    "Index",
                    vm);
            }
        }
    }
}