using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Application.Services
{
    public class FriendService : IFriendService
    {
        private readonly IFriendshipRepository _friendshipRepository;

        private readonly IPostService _postService;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IMapper _mapper;

        public FriendService(
            IFriendshipRepository friendshipRepository,
            IPostService postService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _friendshipRepository = friendshipRepository;
            _postService = postService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<FriendProfileViewModel?> GetFriendProfileAsync(string currentUserId, string targetUserId)
        {
            var isFriend = await AreFriendsAsync(currentUserId, targetUserId);
            var targetUser = await _userManager.FindByIdAsync(targetUserId);

            if (!isFriend || targetUser == null || !targetUser.EmailConfirmed)
            {
                return null;
            }

            return new FriendProfileViewModel
            {
                UserId = targetUser.Id,
                FirstName = targetUser.FirstName,
                LastName = targetUser.LastName,
                UserName = targetUser.UserName ?? string.Empty,
                ProfilePicture = targetUser.ProfilePictureUrl,
                MutualFriends = await GetMutualFriendsAsync(currentUserId, targetUserId),
                Posts = _mapper.Map<List<PostViewModel>>(
                    await _postService.GetFeedByAuthorsAsync(new List<string> { targetUserId }, currentUserId))
            };
        }

        public async Task<string?> GetFriendUserNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName;
        }

        public async Task<bool> AreFriendsAsync(string userIdA, string userIdB)
        {
            var friendship = await _friendshipRepository.GetBetweenAsync(userIdA, userIdB);
            return friendship is { IsActive: true };
        }

        private async Task<List<(Friendship Friendship, ApplicationUser Other)>> GetActiveFriendPairsAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetActiveByUserAsync(userId);

            return friendships
                .Select(f => (f, f.UserOneId == userId ? f.UserTwo : f.UserOne))
                .Where(x => x.Item2 != null && x.Item2.EmailConfirmed)
                .ToList();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(string userId, string? search = null)
        {
            var pairs = await GetActiveFriendPairsAsync(userId);

            var result = new List<FriendDto>();

            foreach (var (friendship, other) in pairs)
            {
                var dto = _mapper.Map<FriendDto>(other);
                dto.FriendshipId = friendship.Id;
                dto.MutualFriendsCount = await GetMutualFriendsCountAsync(userId, other.Id);
                result.Add(dto);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                result = result.Where(x =>
                        x.FirstName.ToLowerInvariant().Contains(term) ||
                        x.LastName.ToLowerInvariant().Contains(term) ||
                        x.UserName.ToLowerInvariant().Contains(term))
                    .ToList();
            }

            return result.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
        }

        public async Task<int> GetFriendsCountAsync(string userId)
        {
            var pairs = await GetActiveFriendPairsAsync(userId);
            return pairs.Count;
        }

        public async Task<List<FriendDto>> GetMutualFriendsAsync(string userIdA, string userIdB)
        {
            var friendIdsA = (await GetActiveFriendPairsAsync(userIdA))
                .Select(x => x.Other.Id)
                .ToHashSet();

            var pairsB = await GetActiveFriendPairsAsync(userIdB);

            return pairsB
                .Where(x => friendIdsA.Contains(x.Other.Id) && x.Other.Id != userIdA && x.Other.Id != userIdB)
                .Select(x => _mapper.Map<FriendDto>(x.Other))
                .ToList();
        }

        public async Task<int> GetMutualFriendsCountAsync(string userIdA, string userIdB)
        {
            return (await GetMutualFriendsAsync(userIdA, userIdB)).Count;
        }

        public async Task<ServiceResult> RemoveFriendAsync(string userId, string friendId)
        {
            var friendship = await _friendshipRepository.GetBetweenAsync(userId, friendId);

            if (friendship == null || !friendship.IsActive)
            {
                return new ServiceResult
                {
                    Succeeded = false,
                    Message = "La amistad seleccionada ya no se encuentra disponible."
                };
            }

            friendship.IsActive = false;
            friendship.RemovedDate = DateTime.UtcNow;
            _friendshipRepository.Update(friendship);
            await _friendshipRepository.SaveChangesAsync();

            return new ServiceResult
            {
                Succeeded = true,
                Message = "La amistad fue eliminada correctamente."
            };
        }

        public async Task<List<PostDto>> GetFriendsFeedAsync(string userId, PostFilterDto? filter = null)
        {
            var pairs = await GetActiveFriendPairsAsync(userId);
            var friendIds = pairs.Select(x => x.Other.Id).ToList();

            if (!string.IsNullOrWhiteSpace(filter?.FriendUserId))
            {
                friendIds = friendIds.Contains(filter.FriendUserId)
                    ? new List<string> { filter.FriendUserId }
                    : new List<string>();
            }

            return await _postService.GetFeedByAuthorsAsync(friendIds, userId, filter);
        }

        public async Task<int> GetVisibleFriendsPostCountAsync(string userId)
        {
            var feed = await GetFriendsFeedAsync(userId);
            return feed.Count;
        }
    }
}
