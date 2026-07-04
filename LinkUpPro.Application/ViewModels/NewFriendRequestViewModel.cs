using LinkUpPro.Application.DTOs;

namespace LinkUpPro.Application.ViewModels
{
    public class NewFriendRequestViewModel
    {
        public string? SearchText { get; set; }

        public List<FriendDto> AvailableUsers { get; set; } = new();
    }
}
