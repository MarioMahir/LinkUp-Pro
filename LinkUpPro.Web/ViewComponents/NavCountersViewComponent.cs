using LinkUpPro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUpPro.Web.ViewComponents
{
    public class NavCountersViewComponent : ViewComponent
    {
        private readonly IFriendRequestService _friendRequestService;

        private readonly INotificationService _notificationService;

        public NavCountersViewComponent(
            IFriendRequestService friendRequestService,
            INotificationService notificationService)
        {
            _friendRequestService = friendRequestService;
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string type)
        {
            var userId = UserClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);

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
