using LinkUpPro.Application.Interfaces;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Repositories
{
    public class BattleshipRepository
        : GenericRepository<BattleshipGame>, IBattleshipRepository
    {
        public BattleshipRepository(AppDbContext context)
            : base(context)
        {
        }

        private IQueryable<BattleshipGame> BaseQuery()
        {
            return _context.BattleshipGames
                .Include(g => g.PlayerOne)
                .Include(g => g.PlayerTwo)
                .Include(g => g.Ships)
                .Include(g => g.Attacks);
        }

        public async Task<BattleshipGame?> GetByIdWithDetailsAsync(int id)
        {
            return await BaseQuery().FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<BattleshipGame>> GetActiveByUserAsync(string userId)
        {
            return await BaseQuery()
                .Where(g =>
                    (g.PlayerOneId == userId || g.PlayerTwoId == userId) &&
                    g.Status != "Finished")
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<BattleshipGame>> GetHistoryByUserAsync(string userId)
        {
            return await BaseQuery()
                .Where(g =>
                    (g.PlayerOneId == userId || g.PlayerTwoId == userId) &&
                    g.Status == "Finished")
                .OrderByDescending(g => g.FinishedDate)
                .ToListAsync();
        }

        public async Task<BattleshipGame?> GetActiveBetweenAsync(string userIdA, string userIdB)
        {
            return await BaseQuery()
                .FirstOrDefaultAsync(g =>
                    g.Status != "Finished" &&
                    ((g.PlayerOneId == userIdA && g.PlayerTwoId == userIdB) ||
                     (g.PlayerOneId == userIdB && g.PlayerTwoId == userIdA)));
        }
    }
}
