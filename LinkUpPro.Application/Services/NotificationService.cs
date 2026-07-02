using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        private readonly IGenericService<Notification> _genericService;

        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IGenericService<Notification> genericService,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _genericService = genericService;
            _mapper = mapper;
        }

        public async Task<List<NotificationDto>> GetAllAsync(string userId)
        {
            var notifications = await _notificationRepository.GetAllByUserAsync(userId);

            var dtos = _mapper.Map<List<NotificationDto>>(notifications);

            for (var i = 0; i < notifications.Count; i++)
            {
                dtos[i].Description = BuildDescription(notifications[i]);
            }

            return dtos;
        }

        private static string BuildDescription(Notification n)
        {
            var actor = n.Actor?.UserName ?? "Alguien";

            return n.Type switch
            {
                "Comment" => $"{actor} comentó tu publicación.",
                "Reply" => $"{actor} respondió tu comentario.",
                "Reaction" => n.ReactionIsLike == true
                    ? $"{actor} reaccionó con Me gusta a tu publicación."
                    : $"{actor} reaccionó con No me gusta a tu publicación.",
                _ => $"{actor} interactuó con tu publicación."
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task CreateAsync(
            string recipientId,
            string actorId,
            string type,
            int postId,
            int? commentId = null,
            bool? reactionIsLike = null)
        {
            if (recipientId == actorId)
            {
                return;
            }

            // Operación común de persistencia delegada al servicio genérico.
            await _genericService.AddAsync(new Notification
            {
                RecipientId = recipientId,
                ActorId = actorId,
                Type = type,
                PostId = postId,
                CommentId = commentId,
                ReactionIsLike = reactionIsLike,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });
        }

        public async Task<ServiceResult> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _genericService.GetByIdAsync(notificationId);

            if (notification == null || notification.RecipientId != userId)
            {
                return new ServiceResult
                {
                    Succeeded = false,
                    Message = "No posee permisos para realizar esta acción."
                };
            }

            notification.IsRead = true;
            await _genericService.UpdateAsync(notification);

            return new ServiceResult { Succeeded = true };
        }

        public async Task<ServiceResult> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _genericService.FindAsync(
                n => n.RecipientId == userId && !n.IsRead);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
            }

            await _notificationRepository.SaveChangesAsync();

            return new ServiceResult { Succeeded = true };
        }
    }
}
