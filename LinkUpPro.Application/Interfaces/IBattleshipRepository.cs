using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IBattleshipRepository : IGenericRepository<BattleshipGame>
    {
        Task<BattleshipGame?> GetByIdWithDetailsAsync(int id);

        Task<List<BattleshipGame>> GetActiveByUserAsync(string userId);

        Task<List<BattleshipGame>> GetHistoryByUserAsync(string userId);

        Task<BattleshipGame?> GetActiveBetweenAsync(string userIdA, string userIdB);
    }
}
