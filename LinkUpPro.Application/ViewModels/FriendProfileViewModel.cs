using LinkUpPro.Application.DTOs;

namespace LinkUpPro.Application.ViewModels
{
    public class FriendProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }

        public List<FriendDto> MutualFriends { get; set; } = new();

        public List<PostViewModel> Posts { get; set; } = new();
    }
}
