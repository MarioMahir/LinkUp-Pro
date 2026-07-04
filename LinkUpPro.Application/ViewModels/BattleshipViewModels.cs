using LinkUpPro.Application.DTOs;

namespace LinkUpPro.Application.ViewModels
{
    public class BattleshipHomeViewModel
    {
        public List<BattleshipGameSummaryDto> ActiveGames { get; set; } = new();

        public List<BattleshipGameSummaryDto> History { get; set; } = new();

        public int TotalGames { get; set; }

        public int WonGames { get; set; }

        public int LostGames { get; set; }
    }

    public class BattleshipNewGameViewModel
    {
        public string? SearchText { get; set; }

        public List<FriendDto> Opponents { get; set; } = new();
    }

    public class BattleshipBoardViewModel
    {
        public int GameId { get; set; }

        public string OpponentUserName { get; set; } = string.Empty;

        public List<List<BoardCellViewModel>> Board { get; set; } = new();

        public bool WaitingForOpponentSetup { get; set; }

        public bool IsMyTurn { get; set; }
    }

    public class ShipSelectionViewModel
    {
        public int GameId { get; set; }

        public List<ShipOptionDto> RemainingShips { get; set; } = new();

        public List<List<BoardCellViewModel>> Board { get; set; } = new();
    }

    public class PlaceShipViewModel
    {
        public int GameId { get; set; }

        public int Length { get; set; }

        public List<List<BoardCellViewModel>> Board { get; set; } = new();
    }

    public class SelectDirectionViewModel
    {
        public int GameId { get; set; }

        public int Length { get; set; }

        public int Row { get; set; }

        public int Col { get; set; }
    }
}
