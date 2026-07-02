namespace LinkUpPro.Application.DTOs
{
    public class PostFilterDto
    {
        public string? SearchText { get; set; }

        public string? ContentType { get; set; }
        // All | Image | YouTube

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? EditedStatus { get; set; }
        // All | Edited | NotEdited

        public string? FriendUserId { get; set; }
    }
}
