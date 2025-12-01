namespace MarketStat.MappingProfiles.Dimensions;

using AutoMapper;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.Dimensions.DimDate;

public class DimDateProfile : Profile
{
    public DimDateProfile()
    {
        CreateMap<DimDate, DimDateDto>();

        CreateMap<CreateDimDateDto, DimDate>();

        CreateMap<UpdateDimDateDto, DimDate>()
            .ForMember(dest => dest.DateId, opt => opt.Ignore());
    }
}
