namespace MarketStat.MappingProfiles.Dimensions;

using AutoMapper;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.Dimensions.DimJob;

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
