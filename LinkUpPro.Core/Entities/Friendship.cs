namespace LinkUpPro.Core.Entities
{
    public class Friendship
    {
        public int Id { get; set; }

        public string UserOneId { get; set; } = string.Empty;

        public ApplicationUser UserOne { get; set; } = null!;

        public string UserTwoId { get; set; } = string.Empty;

        public ApplicationUser UserTwo { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? RemovedDate { get; set; }
    }
}
