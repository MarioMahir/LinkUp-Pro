namespace LinkUpPro.Core.Entities
{
    public class Friendship
    {
        public int Id { get; set; }

        public string UserOneId { get; set; }

        public ApplicationUser UserOne { get; set; }

        public string UserTwoId { get; set; }

        public ApplicationUser UserTwo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? RemovedDate { get; set; }
    }
}
