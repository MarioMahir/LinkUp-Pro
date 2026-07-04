namespace LinkUpPro.Core.Entities
{
    public class Attack
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public BattleshipGame Game { get; set; }

        public string AttackerId { get; set; }

        public int Row { get; set; }

        public int Col { get; set; }

        public bool WasHit { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
