namespace LinkUpPro.Application.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? YoutubeUrl { get; set; }

        public string Privacy { get; set; } = string.Empty;

        public bool AllowComments { get; set; }

        public bool IsEdited { get; set; }

        public DateTime CreatedDate { get; set; }

        public string AuthorId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string AuthorFullName { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }

        public int LikesCount { get; set; }

        public int DislikesCount { get; set; }

        public bool? CurrentUserReactionIsLike { get; set; }

        public List<CommentViewModel> Comments { get; set; } = new();
    }
}
