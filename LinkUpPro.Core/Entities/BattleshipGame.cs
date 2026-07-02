namespace LinkUpPro.Core.Entities
{
    public class BattleshipGame
    {
        public int Id { get; set; }

        public string PlayerOneId { get; set; }

        public ApplicationUser PlayerOne { get; set; }

        public string PlayerTwoId { get; set; }

        public ApplicationUser PlayerTwo { get; set; }

        public string Status { get; set; } = "SettingUp";
        // SettingUp | Attacking | Finished

        // Combinación ordenada de ambos jugadores, usada por un índice único
        // filtrado para impedir más de una partida activa simultánea entre
        // los mismos dos usuarios bajo condiciones de concurrencia.
        public string PairKey { get; set; } = string.Empty;

        public bool PlayerOneShipsReady { get; set; }

        public bool PlayerTwoShipsReady { get; set; }

        public string? CurrentTurnUserId { get; set; }

        public DateTime? TurnAssignedDate { get; set; }

        public string? WinnerId { get; set; }

        public string? FinishReason { get; set; }
        // Won | Surrendered | Forfeited

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? FinishedDate { get; set; }

        public ICollection<Ship> Ships { get; set; } = new List<Ship>();

        public ICollection<Attack> Attacks { get; set; } = new List<Attack>();
    }
}
