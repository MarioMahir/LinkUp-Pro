using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class FriendshipRepository
        : GenericRepository<Friendship>, IFriendshipRepository
    {
        public FriendshipRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Friendship?> GetBetweenAsync(string userIdA, string userIdB)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserOneId == userIdA && f.UserTwoId == userIdB) ||
                    (f.UserOneId == userIdB && f.UserTwoId == userIdA));
        }

        public async Task<List<Friendship>> GetActiveByUserAsync(string userId)
        {
            return await _context.Friendships
                .Include(f => f.UserOne)
                .Include(f => f.UserTwo)
                .Where(f =>
                    f.IsActive &&
                    (f.UserOneId == userId || f.UserTwoId == userId))
                .ToListAsync();
        }
    }
}
