using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUpPro.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPostService _postService;

        private readonly IFriendRequestService _friendRequestService;

        private readonly INotificationService _notificationService;

        private readonly IMapper _mapper;

        public HomeController(
            IPostService postService,
            IFriendRequestService friendRequestService,
            INotificationService notificationService,
            IMapper mapper)
        {
            _postService = postService;
            _friendRequestService = friendRequestService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private async Task PopulateCountersAsync(HomeViewModel vm)
        {
            vm.PendingRequestsCount = await _friendRequestService.GetPendingCountAsync(CurrentUserId);
            vm.UnreadNotificationsCount = await _notificationService.GetUnreadCountAsync(CurrentUserId);
        }

        [HttpGet]
        public async Task<IActionResult> Index(HomeViewModel vm)
        {
            ViewData["CurrentUserId"] = CurrentUserId;
            ViewData["ReturnUrl"] = Url.Action("Index");

            var hasFilter = !string.IsNullOrWhiteSpace(vm.SearchText)
                || !string.IsNullOrWhiteSpace(vm.ContentType)
                || vm.DateFrom.HasValue
                || vm.DateTo.HasValue
                || !string.IsNullOrWhiteSpace(vm.EditedStatus);

            vm.HasFilter = hasFilter;

            if (vm.DateFrom.HasValue && vm.DateTo.HasValue && vm.DateFrom > vm.DateTo)
            {
                TempData["Error"] = "La fecha inicial no puede ser posterior a la fecha final.";
                vm.Posts = _mapper.Map<List<PostViewModel>>(await _postService.GetOwnFeedAsync(CurrentUserId));
                await PopulateCountersAsync(vm);
                return View(vm);
            }

            var filter = new PostFilterDto
            {
                SearchText = vm.SearchText,
                ContentType = vm.ContentType,
                DateFrom = vm.DateFrom,
                DateTo = vm.DateTo,
                EditedStatus = vm.EditedStatus
            };

            var posts = await _postService.GetOwnFeedAsync(CurrentUserId, hasFilter ? filter : null);
            vm.Posts = _mapper.Map<List<PostViewModel>>(posts);

            if (hasFilter && vm.Posts.Count == 0)
            {
                TempData["Error"] = "No se encontraron publicaciones que coincidan con los criterios seleccionados.";
            }

            await PopulateCountersAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(HomeViewModel vm)
        {
            var result = await _postService.CreateAsync(vm.NewPost, CurrentUserId);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Message"] = result.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postService.GetByIdAsync(id, CurrentUserId);

            if (post == null || post.AuthorId != CurrentUserId)
            {
                TempData["Error"] = "No posee permisos para editar esta publicación.";
                return RedirectToAction("Index");
            }

            var vm = new SavePostViewModel
            {
                Id = post.Id,
                Content = post.Content,
                ContentType = post.ContentType,
                CurrentImageUrl = post.ImageUrl,
                YoutubeUrl = post.YoutubeUrl,
                Privacy = post.Privacy,
                AllowComments = post.AllowComments
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavePostViewModel vm)
        {
            var result = await _postService.EditAsync(vm, CurrentUserId);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Message);
                return View(vm);
            }

            TempData["Message"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var post = await _postService.GetByIdAsync(id, CurrentUserId);

            if (post == null || post.AuthorId != CurrentUserId)
            {
                TempData["Error"] = "No posee permisos para eliminar esta publicación.";
                return RedirectToAction("Index");
            }

            var vm = new ConfirmActionViewModel
            {
                Title = "Eliminar publicación",
                Message = "¿Está seguro que desea eliminar esta publicación?",
                FormController = "Home",
                FormAction = "Delete",
                HiddenFields = new Dictionary<string, string> { ["id"] = id.ToString() },
                CancelUrl = Url.Action("Index")!,
                ConfirmButtonText = "Aceptar"
            };

            return View("Confirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _postService.DeleteAsync(id, CurrentUserId);

            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postService.GetByIdAsync(id, CurrentUserId);

            if (post == null)
            {
                TempData["Error"] = "No posee permisos para visualizar esta publicación.";
                return RedirectToAction("Index");
            }

            ViewData["CurrentUserId"] = CurrentUserId;
            ViewData["ReturnUrl"] = Url.Action("Details", new { id });

            return View(_mapper.Map<PostViewModel>(post));
        }

        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content, int? parentCommentId, string? returnUrl)
        {
            var result = await _postService.AddCommentAsync(postId, content, CurrentUserId, parentCommentId);

            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;

            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string content, string? returnUrl)
        {
            var result = await _postService.EditCommentAsync(commentId, content, CurrentUserId);

            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;

            return RedirectBack(returnUrl);
        }

        [HttpGet]
        public IActionResult DeleteCommentConfirm(int commentId, string? returnUrl)
        {
            var vm = new ConfirmActionViewModel
            {
                Title = "Eliminar comentario",
                Message = "¿Está seguro que desea eliminar este comentario?",
                FormController = "Home",
                FormAction = "DeleteComment",
                HiddenFields = new Dictionary<string, string>
                {
                    ["commentId"] = commentId.ToString(),
                    ["returnUrl"] = returnUrl ?? string.Empty
                },
                CancelUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index")! : returnUrl
            };

            return View("Confirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, string? returnUrl)
        {
            var result = await _postService.DeleteCommentAsync(commentId, CurrentUserId);

            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;

            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> React(int postId, bool isLike, string? returnUrl)
        {
            var result = await _postService.SetReactionAsync(postId, CurrentUserId, isLike);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.Message;
            }

            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReaction(int postId, string? returnUrl)
        {
            await _postService.RemoveReactionAsync(postId, CurrentUserId);

            return RedirectBack(returnUrl);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new LinkUpPro.Web.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
