namespace LinkUpPro.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string RecipientId { get; set; } = string.Empty;

        public ApplicationUser Recipient { get; set; } = null!;

        public string ActorId { get; set; } = string.Empty;

        public ApplicationUser Actor { get; set; } = null!;

        public string Type { get; set; } = string.Empty;

        public bool? ReactionIsLike { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; } = null!;

        public int? CommentId { get; set; }

        public Comment? Comment { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
