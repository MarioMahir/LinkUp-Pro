using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);

        Task<List<Post>> GetAllByUserAsync(string userId);

        Task SaveChangesAsync();
    }
}
