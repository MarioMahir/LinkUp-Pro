namespace LinkUpPro.Application.ViewModels
{
    public class BoardCellViewModel
    {
        public int Row { get; set; }

        public int Col { get; set; }

        public bool HasShip { get; set; }

        public bool IsAttacked { get; set; }

        public bool WasHit { get; set; }

        public bool IsSunk { get; set; }
    }
}
