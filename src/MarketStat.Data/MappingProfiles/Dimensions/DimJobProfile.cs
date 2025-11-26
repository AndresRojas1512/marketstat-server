using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimJobProfile : Profile
{
    public DimJobProfile()
    {
        CreateMap<DimJob, DimJobDto>()
            .ForMember(dest => dest.IndustryField, opt => opt.MapFrom(src => src.IndustryField));
        CreateMap<CreateDimJobDto, DimJob>();
        CreateMap<UpdateDimJobDto, DimJob>();
    }
}