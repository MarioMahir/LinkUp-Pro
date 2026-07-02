using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Web.ViewComponents
{
    public class NavCountersViewComponent : ViewComponent
    {
        private readonly IFriendRequestService _friendRequestService;

        private readonly INotificationService _notificationService;

        private readonly UserManager<ApplicationUser> _userManager;

        public NavCountersViewComponent(
            IFriendRequestService friendRequestService,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _friendRequestService = friendRequestService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string type)
        {
            var userId = _userManager.GetUserId(UserClaimsPrincipal);

            if (string.IsNullOrEmpty(userId))
            {
                return Content(string.Empty);
            }

            var count = type == "requests"
                ? await _friendRequestService.GetPendingCountAsync(userId)
                : await _notificationService.GetUnreadCountAsync(userId);

            return View(count);
        }
    }
}
