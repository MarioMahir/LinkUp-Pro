namespace LinkUpPro.Application.DTOs
{
    public class BoardCellDto
    {
        public int Row { get; set; }

        public int Col { get; set; }

        public bool HasShip { get; set; }

        public bool IsAttacked { get; set; }

        public bool WasHit { get; set; }

        public bool IsSunk { get; set; }
    }

    public class BattleshipGameSummaryDto
    {
        public int Id { get; set; }

        public string OpponentId { get; set; } = string.Empty;

        public string OpponentUserName { get; set; } = string.Empty;

        public DateTime StartedDate { get; set; }

        public DateTime? FinishedDate { get; set; }

        public double HoursElapsed { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsMyTurn { get; set; }

        public bool Won { get; set; }

        public string WinnerDisplay { get; set; } = string.Empty;
    }

    public class ShipOptionDto
    {
        public int Length { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    public class BattleshipGameStateDto
    {
        public int GameId { get; set; }

        public string OpponentUserName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool MyShipsReady { get; set; }

        public bool OpponentShipsReady { get; set; }

        public bool IsMyTurn { get; set; }

        public string? WinnerId { get; set; }

        public bool Won { get; set; }
    }

    public class BattleshipResultDto
    {
        public int GameId { get; set; }

        public string OpponentUserName { get; set; } = string.Empty;

        public bool Won { get; set; }

        public List<List<BoardCellDto>> MyAttackBoard { get; set; } = new();

        public List<List<BoardCellDto>> OpponentAttackBoard { get; set; } = new();

        public List<List<BoardCellDto>> MyShipsBoard { get; set; } = new();
    }
}
