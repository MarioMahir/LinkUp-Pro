using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<List<Post>> GetAllByUserAsync(string userId);

        Task<Post?> GetByIdWithDetailsAsync(int id);

        Task<List<Post>> GetVisibleByFriendAsync(string friendId);
    }
}
