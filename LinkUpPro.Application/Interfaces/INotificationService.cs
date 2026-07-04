using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;

namespace LinkUpPro.Application.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAllAsync(string userId);

        Task<int> GetUnreadCountAsync(string userId);

        Task CreateAsync(
            string recipientId,
            string actorId,
            string type,
            int postId,
            int? commentId = null,
            bool? reactionIsLike = null);

        Task<ServiceResult> MarkAsReadAsync(int notificationId, string userId);

        Task<ServiceResult> MarkAllAsReadAsync(string userId);
    }
}
