namespace LinkUpPro.Application.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public int PostId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }

        public bool AuthorIsFriendOfViewer { get; set; }

        public int? ParentCommentId { get; set; }

        public List<CommentDto> Replies { get; set; } = new();
    }
}
