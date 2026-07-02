namespace LinkUpPro.Application.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public int PostId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string? ProfilePicture { get; set; }

        public bool AuthorIsFriendOfViewer { get; set; }

        public int? ParentCommentId { get; set; }

        public List<CommentViewModel> Replies { get; set; } = new();
    }
}
