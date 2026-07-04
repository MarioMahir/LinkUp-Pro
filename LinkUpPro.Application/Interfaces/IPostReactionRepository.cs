using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IPostReactionRepository : IGenericRepository<PostReaction>
    {
        Task<PostReaction?> GetByUserAndPostAsync(string userId, int postId);
    }
}
