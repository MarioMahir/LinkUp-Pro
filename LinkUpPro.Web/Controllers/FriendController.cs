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
    public class FriendController : Controller
    {
        private readonly IFriendService _friendService;

        private readonly IFriendRequestService _friendRequestService;

        private readonly IMapper _mapper;

        public FriendController(
            IFriendService friendService,
            IFriendRequestService friendRequestService,
            IMapper mapper)
        {
            _friendService = friendService;
            _friendRequestService = friendRequestService;
            _mapper = mapper;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> Index(FriendFeedViewModel vm)
        {
            ViewData["CurrentUserId"] = CurrentUserId;
            ViewData["ReturnUrl"] = Url.Action("Index");

            vm.TotalFriends = await _friendService.GetFriendsCountAsync(CurrentUserId);
            vm.TotalVisiblePosts = await _friendService.GetVisibleFriendsPostCountAsync(CurrentUserId);

            vm.AllFriends = await _friendService.GetFriendsAsync(CurrentUserId);
            vm.Friends = await _friendService.GetFriendsAsync(CurrentUserId, vm.FriendSearchText);

            if (vm.DateFrom.HasValue && vm.DateTo.HasValue && vm.DateFrom > vm.DateTo)
            {
                TempData["Error"] = "La fecha inicial no puede ser posterior a la fecha final.";
                var allFeed = await _friendService.GetFriendsFeedAsync(CurrentUserId);
                vm.Posts = _mapper.Map<List<PostViewModel>>(allFeed);
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.FriendUserId)
                && !vm.AllFriends.Any(f => f.UserId == vm.FriendUserId))
            {
                TempData["Error"] = "El usuario seleccionado ya no forma parte de su lista de amigos.";
                vm.FriendUserId = null;
                var allFeed = await _friendService.GetFriendsFeedAsync(CurrentUserId);
                vm.Posts = _mapper.Map<List<PostViewModel>>(allFeed);
                return View(vm);
            }

            var hasFilter = !string.IsNullOrWhiteSpace(vm.SearchText)
                || !string.IsNullOrWhiteSpace(vm.ContentType)
                || vm.DateFrom.HasValue
                || vm.DateTo.HasValue
                || !string.IsNullOrWhiteSpace(vm.EditedStatus)
                || !string.IsNullOrWhiteSpace(vm.FriendUserId);

            var filter = new PostFilterDto
            {
                SearchText = vm.SearchText,
                ContentType = vm.ContentType,
                DateFrom = vm.DateFrom,
                DateTo = vm.DateTo,
                EditedStatus = vm.EditedStatus,
                FriendUserId = vm.FriendUserId
            };

            var feed = await _friendService.GetFriendsFeedAsync(CurrentUserId, hasFilter ? filter : null);
            vm.Posts = _mapper.Map<List<PostViewModel>>(feed);

            if (hasFilter && vm.Posts.Count == 0)
            {
                TempData["Error"] = "No se encontraron publicaciones que coincidan con los criterios seleccionados.";
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string userId)
        {
            var vm = await _friendService.GetFriendProfileAsync(CurrentUserId, userId);

            if (vm == null)
            {
                TempData["Error"] = "No posee permisos para visualizar el perfil de este usuario.";
                return RedirectToAction("Index");
            }

            ViewData["CurrentUserId"] = CurrentUserId;
            ViewData["ReturnUrl"] = Url.Action("Profile", new { userId });

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveFriendConfirm(string friendId)
        {
            var friendUserName = await _friendService.GetFriendUserNameAsync(friendId);

            var vm = new ConfirmActionViewModel
            {
                Title = "Eliminar amigo",
                Message = $"¿Está seguro que desea eliminar de sus amigos al usuario {friendUserName}?",
                FormController = "Friend",
                FormAction = "RemoveFriend",
                HiddenFields = new Dictionary<string, string> { ["friendId"] = friendId },
                CancelUrl = Url.Action("Index")!
            };

            return View("Confirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var result = await _friendService.RemoveFriendAsync(CurrentUserId, friendId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var vm = new FriendRequestsViewModel
            {
                Received = await _friendRequestService.GetPendingReceivedAsync(CurrentUserId),
                Sent = await _friendRequestService.GetSentAsync(CurrentUserId),
                ReceivedHistory = await _friendRequestService.GetReceivedHistoryAsync(CurrentUserId)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> NewRequest(string? search)
        {
            var vm = new NewFriendRequestViewModel
            {
                SearchText = search,
                AvailableUsers = await _friendRequestService.GetAvailableUsersAsync(CurrentUserId, search)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string selectedUserId)
        {
            if (string.IsNullOrWhiteSpace(selectedUserId))
            {
                TempData["Error"] = "Debe seleccionar un usuario para enviar la solicitud de amistad.";
                return RedirectToAction("NewRequest");
            }

            var result = await _friendRequestService.SendRequestAsync(CurrentUserId, selectedUserId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Requests");
        }

        [HttpGet]
        public async Task<IActionResult> AcceptConfirm(int id)
        {
            var request = (await _friendRequestService.GetPendingReceivedAsync(CurrentUserId))
                .FirstOrDefault(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Esta solicitud ya no se encuentra disponible para ser aceptada.";
                return RedirectToAction("Requests");
            }

            return View("Confirm", new ConfirmActionViewModel
            {
                Title = "Aceptar solicitud de amistad",
                Message = $"¿Está seguro que desea aceptar la solicitud de amistad del usuario {request.OtherUserName}?",
                FormController = "Friend",
                FormAction = "Accept",
                HiddenFields = new Dictionary<string, string> { ["id"] = id.ToString() },
                CancelUrl = Url.Action("Requests")!,
                ConfirmButtonClass = "btn-success"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var result = await _friendRequestService.AcceptRequestAsync(id, CurrentUserId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Requests");
        }

        [HttpGet]
        public async Task<IActionResult> RejectConfirm(int id)
        {
            var request = (await _friendRequestService.GetPendingReceivedAsync(CurrentUserId))
                .FirstOrDefault(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Esta solicitud ya no se encuentra disponible.";
                return RedirectToAction("Requests");
            }

            return View("Confirm", new ConfirmActionViewModel
            {
                Title = "Rechazar solicitud de amistad",
                Message = $"¿Está seguro que desea rechazar la solicitud de amistad del usuario {request.OtherUserName}?",
                FormController = "Friend",
                FormAction = "Reject",
                HiddenFields = new Dictionary<string, string> { ["id"] = id.ToString() },
                CancelUrl = Url.Action("Requests")!
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _friendRequestService.RejectRequestAsync(id, CurrentUserId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Requests");
        }

        [HttpGet]
        public async Task<IActionResult> CancelConfirm(int id)
        {
            var request = (await _friendRequestService.GetSentAsync(CurrentUserId))
                .FirstOrDefault(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Esta solicitud ya no se encuentra disponible.";
                return RedirectToAction("Requests");
            }

            return View("Confirm", new ConfirmActionViewModel
            {
                Title = "Cancelar solicitud de amistad",
                Message = $"¿Está seguro que desea cancelar la solicitud de amistad enviada a {request.OtherUserName}?",
                FormController = "Friend",
                FormAction = "Cancel",
                HiddenFields = new Dictionary<string, string> { ["id"] = id.ToString() },
                CancelUrl = Url.Action("Requests")!
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _friendRequestService.CancelRequestAsync(id, CurrentUserId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Requests");
        }

        [HttpGet]
        public IActionResult HideFromHistoryConfirm(int id)
        {
            return View("Confirm", new ConfirmActionViewModel
            {
                Title = "Eliminar del historial",
                Message = "¿Está seguro que desea eliminar esta solicitud de su historial?",
                FormController = "Friend",
                FormAction = "HideFromHistory",
                HiddenFields = new Dictionary<string, string> { ["id"] = id.ToString() },
                CancelUrl = Url.Action("Requests")!
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideFromHistory(int id)
        {
            var result = await _friendRequestService.HideFromHistoryAsync(id, CurrentUserId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Requests");
        }

        [HttpGet]
        public IActionResult HideFromReceiverHistoryConfirm(int id)
        {
            return View("Confirm", new ConfirmActionViewModel
            {
                Title = "Eliminar del historial",
                Message = "¿Está seguro que desea eliminar esta solicitud de su historial?",
                FormController = "Friend",
                FormAction = "HideFromReceiverHistory",
                HiddenFields = new Dictionary<string, string> { ["id"] = id.ToString() },
                CancelUrl = Url.Action("Requests")!
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideFromReceiverHistory(int id)
        {
            var result = await _friendRequestService.HideFromReceiverHistoryAsync(id, CurrentUserId);
            TempData[result.Succeeded ? "Message" : "Error"] = result.Message;
            return RedirectToAction("Requests");
        }
    }
}
