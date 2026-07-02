namespace LinkUpPro.Application.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }

        public string? ImageUrl { get; set; }

        public string? YoutubeUrl { get; set; }

        public string Privacy { get; set; }

        public bool AllowComments { get; set; }

        public bool IsEdited { get; set; }

        public DateTime CreatedDate { get; set; }

        public string AuthorId { get; set; }

        public string UserName { get; set; }

        public string AuthorFullName { get; set; }

        public string? ProfilePicture { get; set; }

        public int LikesCount { get; set; }

        public int DislikesCount { get; set; }

        public bool? CurrentUserReactionIsLike { get; set; }

        public List<CommentViewModel> Comments { get; set; } = new();
    }
}
