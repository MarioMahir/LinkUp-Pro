using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class PostRepository
        : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Post post)
        {
            await _context.Posts
                .AddAsync(post);
        }

        public async Task<List<Post>>
            GetAllByUserAsync(
            string userId)
        {
            return await _context.Posts
                .Include(x => x.User)
                .Include(x => x.Reactions)
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsDeleted)
                .OrderByDescending(
                    x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context
                .SaveChangesAsync();
        }
    }
}