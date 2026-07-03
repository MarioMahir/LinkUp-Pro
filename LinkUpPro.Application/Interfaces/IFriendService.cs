using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.ViewModels;

namespace LinkUpPro.Application.Interfaces
{
    public interface IFriendService
    {
        Task<FriendProfileViewModel?> GetFriendProfileAsync(string currentUserId, string targetUserId);

        Task<string?> GetFriendUserNameAsync(string userId);

        Task<List<FriendDto>> GetFriendsAsync(string userId, string? search = null);

        Task<int> GetFriendsCountAsync(string userId);

        Task<List<FriendDto>> GetMutualFriendsAsync(string userIdA, string userIdB);

        Task<int> GetMutualFriendsCountAsync(string userIdA, string userIdB);

        Task<bool> AreFriendsAsync(string userIdA, string userIdB);

        Task<ServiceResult> RemoveFriendAsync(string userId, string friendId);

        Task<List<PostDto>> GetFriendsFeedAsync(string userId, PostFilterDto? filter = null);

        Task<int> GetVisibleFriendsPostCountAsync(string userId);
    }
}
