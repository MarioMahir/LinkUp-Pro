using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class PostReactionRepository
        : GenericRepository<PostReaction>, IPostReactionRepository
    {
        public PostReactionRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<PostReaction?> GetByUserAndPostAsync(string userId, int postId)
        {
            return await _context.PostReactions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
        }
    }
}
