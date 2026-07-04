using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;

namespace LinkUpPro.Application.Interfaces
{
    public interface IBattleshipService
    {
        Task<List<BattleshipGameSummaryDto>> GetActiveGamesAsync(string userId);

        Task<List<BattleshipGameSummaryDto>> GetHistoryAsync(string userId);

        Task<(int Total, int Won, int Lost)> GetHistorySummaryAsync(string userId);

        Task<List<FriendDto>> GetEligibleOpponentsAsync(string userId, string? search);

        Task<ServiceResult> CreateGameAsync(string userId, string opponentId);

        Task<ServiceResult> SurrenderAsync(int gameId, string userId);

        Task<BattleshipGameStateDto?> GetGameStateAsync(int gameId, string userId);

        Task<List<ShipOptionDto>> GetRemainingShipsAsync(int gameId, string userId);

        Task<List<List<BoardCellDto>>> GetOwnShipsBoardAsync(int gameId, string userId);

        Task<ServiceResult> PlaceShipAsync(
            int gameId, string userId, int length, int startRow, int startCol, string direction);

        Task<List<List<BoardCellDto>>> GetAttackBoardAsync(int gameId, string userId);

        Task<ServiceResult> AttackAsync(int gameId, string userId, int row, int col);

        Task<BattleshipResultDto?> GetResultAsync(int gameId, string userId);
    }
}
