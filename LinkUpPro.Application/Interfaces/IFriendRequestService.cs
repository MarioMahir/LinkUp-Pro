using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;

namespace LinkUpPro.Application.Interfaces
{
    public interface IFriendRequestService
    {
        Task<List<FriendRequestDto>> GetPendingReceivedAsync(string userId);

        Task<List<FriendRequestDto>> GetSentAsync(string userId);

        Task<List<FriendRequestDto>> GetReceivedHistoryAsync(string userId);

        Task<int> GetPendingCountAsync(string userId);

        Task<List<FriendDto>> GetAvailableUsersAsync(string userId, string? search = null);

        Task<ServiceResult> SendRequestAsync(string senderId, string receiverId);

        Task<ServiceResult> AcceptRequestAsync(int requestId, string currentUserId);

        Task<ServiceResult> RejectRequestAsync(int requestId, string currentUserId);

        Task<ServiceResult> CancelRequestAsync(int requestId, string currentUserId);

        Task<ServiceResult> HideFromHistoryAsync(int requestId, string currentUserId);

        Task<ServiceResult> HideFromReceiverHistoryAsync(int requestId, string currentUserId);
    }
}
