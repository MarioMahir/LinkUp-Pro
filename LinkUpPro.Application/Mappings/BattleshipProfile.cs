using AutoMapper;
using LinkUpPro.Application.DTOs;
using LinkUpPro.Application.ViewModels;

namespace LinkUpPro.Application.Mappings
{
    public class BattleshipProfile : Profile
    {
        public BattleshipProfile()
        {
            CreateMap<BoardCellDto, BoardCellViewModel>();
            CreateMap<BattleshipResultDto, BattleshipResultViewModel>();
        }
    }
}
