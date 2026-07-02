using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Mappings
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<PostDto, PostViewModel>();
            CreateMap<CommentDto, CommentViewModel>();

            CreateMap<Post, PostDto>()
                .ForMember(d => d.AuthorId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
                .ForMember(d => d.AuthorFullName, o => o.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
                .ForMember(d => d.ProfilePicture, o => o.MapFrom(s => s.User.ProfilePictureUrl))
                .ForMember(d => d.LikesCount, o => o.MapFrom(s => s.Reactions.Count(r => r.IsLike)))
                .ForMember(d => d.DislikesCount, o => o.MapFrom(s => s.Reactions.Count(r => !r.IsLike)))
                .ForMember(d => d.CurrentUserReactionIsLike, o => o.Ignore())
                .ForMember(d => d.Comments, o => o.Ignore());

            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
                .ForMember(d => d.ProfilePicture, o => o.MapFrom(s => s.User.ProfilePictureUrl))
                .ForMember(d => d.AuthorIsFriendOfViewer, o => o.Ignore())
                .ForMember(d => d.Replies, o => o.Ignore());
        }
    }
}
