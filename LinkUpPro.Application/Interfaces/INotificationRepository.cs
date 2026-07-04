using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetAllByUserAsync(string userId);

        Task<int> GetUnreadCountAsync(string userId);
    }
}
