namespace LinkUpPro.Application.ViewModels
{
    public class BattleshipResultViewModel
    {
        public int GameId { get; set; }

        public string OpponentUserName { get; set; } = string.Empty;

        public bool Won { get; set; }

        public List<List<BoardCellViewModel>> MyAttackBoard { get; set; } = new();

        public List<List<BoardCellViewModel>> OpponentAttackBoard { get; set; } = new();

        public List<List<BoardCellViewModel>> MyShipsBoard { get; set; } = new();
    }
}
