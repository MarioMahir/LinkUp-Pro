namespace LinkUpPro.Core.Entities
{
    public class Ship
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public BattleshipGame Game { get; set; } = null!;

        public string OwnerId { get; set; } = string.Empty;

        public int Length { get; set; }

        public int StartRow { get; set; }

        public int StartCol { get; set; }

        public string Direction { get; set; } = string.Empty;

        public IEnumerable<(int Row, int Col)> GetOccupiedCells()
        {
            for (var i = 0; i < Length; i++)
            {
                yield return Direction switch
                {
                    "Up" => (StartRow - i, StartCol),
                    "Down" => (StartRow + i, StartCol),
                    "Left" => (StartRow, StartCol - i),
                    "Right" => (StartRow, StartCol + i),
                    _ => (StartRow, StartCol)
                };
            }
        }
    }
}
