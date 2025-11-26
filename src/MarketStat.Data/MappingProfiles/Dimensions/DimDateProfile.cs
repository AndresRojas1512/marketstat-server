using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;

namespace MarketStat.MappingProfiles.Dimensions;

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