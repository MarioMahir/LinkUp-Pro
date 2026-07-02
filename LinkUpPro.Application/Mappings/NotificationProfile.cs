using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.ViewModels;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Mappings
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDto>()
                .ForMember(d => d.ActorUserName, o => o.MapFrom(s => s.Actor.UserName))
                .ForMember(d => d.ActorProfilePicture, o => o.MapFrom(s => s.Actor.ProfilePictureUrl))
                .ForMember(d => d.PostIsAvailable, o => o.MapFrom(s => s.Post != null && !s.Post.IsDeleted))
                .ForMember(d => d.Description, o => o.Ignore());

            CreateMap<NotificationDto, NotificationViewModel>();
        }
    }
}
