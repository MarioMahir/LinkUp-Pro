using AutoMapper;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IMapper _mapper;

        public NotificationController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _mapper = mapper;
        }

        private string CurrentUserId => _userManager.GetUserId(User)!;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var notifications = await _notificationService.GetAllAsync(CurrentUserId);
            return View(_mapper.Map<List<NotificationViewModel>>(notifications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id, CurrentUserId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(CurrentUserId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var notifications = await _notificationService.GetAllAsync(CurrentUserId);
            var notification = notifications.FirstOrDefault(n => n.Id == id);

            if (notification == null)
            {
                return RedirectToAction("Index");
            }

            await _notificationService.MarkAsReadAsync(id, CurrentUserId);

            if (!notification.PostIsAvailable)
            {
                TempData["Error"] = "El contenido relacionado con esta notificación ya no se encuentra disponible.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Details", "Home", new { id = notification.PostId });
        }
    }
}
