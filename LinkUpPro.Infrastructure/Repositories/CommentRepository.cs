using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class CommentRepository
        : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<List<Comment>> GetAllByPostAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}
