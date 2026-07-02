using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Core.Entities;

namespace LinkUpPro.Application.Mappings
{
    public class FriendProfile : Profile
    {
        public FriendProfile()
        {
            CreateMap<ApplicationUser, FriendDto>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.FriendshipId, o => o.Ignore())
                .ForMember(d => d.MutualFriendsCount, o => o.Ignore());
        }
    }
}
