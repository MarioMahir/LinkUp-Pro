namespace LinkUpPro.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string RecipientId { get; set; }

        public ApplicationUser Recipient { get; set; }

        public string ActorId { get; set; }

        public ApplicationUser Actor { get; set; }

        public string Type { get; set; }
        // Comment | Reply | Reaction

        public bool? ReactionIsLike { get; set; }

        public int PostId { get; set; }

        public Post Post { get; set; }

        public int? CommentId { get; set; }

        public Comment? Comment { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
