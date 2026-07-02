using LinkUpPro.Application.DTOs;

namespace LinkUpPro.Application.ViewModels
{
    public class FriendFeedViewModel
    {
        public int TotalFriends { get; set; }

        public int TotalVisiblePosts { get; set; }

        public string? SearchText { get; set; }

        public string? ContentType { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? EditedStatus { get; set; }

        public string? FriendUserId { get; set; }

        public string? FriendSearchText { get; set; }

        public List<PostViewModel> Posts { get; set; } = new();

        public List<FriendDto> Friends { get; set; } = new();
    }
}
