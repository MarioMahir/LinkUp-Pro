using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IFriendshipRepository : IGenericRepository<Friendship>
    {
        Task<Friendship?> GetBetweenAsync(string userIdA, string userIdB);

        Task<List<Friendship>> GetActiveByUserAsync(string userId);
    }
}
