namespace LinkUpPro.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string ActorId { get; set; } = string.Empty;

        public string ActorUserName { get; set; } = string.Empty;

        public string? ActorProfilePicture { get; set; }

        public string Type { get; set; } = string.Empty;

        public bool? ReactionIsLike { get; set; }

        public string Description { get; set; } = string.Empty;

        public int PostId { get; set; }

        public int? CommentId { get; set; }

        public bool PostIsAvailable { get; set; } = true;

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
