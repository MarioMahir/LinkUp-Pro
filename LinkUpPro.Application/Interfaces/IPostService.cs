using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.Helpers;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Interfaces
{
    public interface IPostService
    {
        Task<ServiceResult> CreateAsync(SavePostViewModel vm, string userId);

        Task<ServiceResult> EditAsync(SavePostViewModel vm, string userId);

        Task<ServiceResult> DeleteAsync(int postId, string userId);

        Task<PostDto?> GetByIdAsync(int postId, string viewerId);

        Task<List<PostDto>> GetOwnFeedAsync(string userId, PostFilterDto? filter = null);

        Task<List<PostDto>> GetFeedByAuthorsAsync(
            List<string> authorIds,
            string viewerId,
            PostFilterDto? filter = null);

        Task<PostDto> ToDtoAsync(Post post, string viewerId);

        Task<ServiceResult> AddCommentAsync(
            int postId,
            string content,
            string userId,
            int? parentCommentId = null);

        Task<ServiceResult> EditCommentAsync(int commentId, string content, string userId);

        Task<ServiceResult> DeleteCommentAsync(int commentId, string userId);

        Task<ServiceResult> SetReactionAsync(int postId, string userId, bool isLike);

        Task<ServiceResult> RemoveReactionAsync(int postId, string userId);
    }
}
