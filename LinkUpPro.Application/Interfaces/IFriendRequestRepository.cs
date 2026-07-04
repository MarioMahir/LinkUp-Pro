using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
    {
        Task<FriendRequest?> GetPendingBetweenAsync(string userIdA, string userIdB);

        Task<List<FriendRequest>> GetPendingReceivedAsync(string userId);

        Task<List<FriendRequest>> GetVisibleSentAsync(string userId);

        Task<List<FriendRequest>> GetVisibleReceivedHistoryAsync(string userId);
    }
}
