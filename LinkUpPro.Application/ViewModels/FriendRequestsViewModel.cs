using LinkUpPro.Application.DTOs;

namespace LinkUpPro.Application.ViewModels
{
    public class FriendRequestsViewModel
    {
        public List<FriendRequestDto> Received { get; set; } = new();

        public List<FriendRequestDto> Sent { get; set; } = new();
    }
}
