using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class PostRepository
        : GenericRepository<Post>, IPostRepository
    {
        public PostRepository(
            AppDbContext context)
            : base(context)
        {
        }

        public async Task<List<Post>>
            GetAllByUserAsync(
            string userId)
        {
            return await _context.Posts
                .Include(x => x.User)
                .Include(x => x.Reactions)
                .Include(x => x.Comments)
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsDeleted)
                .OrderByDescending(
                    x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Posts
                .Include(x => x.User)
                .Include(x => x.Reactions)
                    .ThenInclude(r => r.User)
                .Include(x => x.Comments)
                    .ThenInclude(c => c.User)
                .Include(x => x.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Post>> GetVisibleByFriendAsync(string friendId)
        {
            return await _context.Posts
                .Include(x => x.User)
                .Include(x => x.Reactions)
                .Include(x => x.Comments)
                .Where(x =>
                    x.UserId == friendId &&
                    !x.IsDeleted &&
                    x.Privacy == "Friends")
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }
    }
}
