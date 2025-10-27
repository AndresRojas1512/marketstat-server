using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimLocationProfile : Profile
{
    public DimLocationProfile()
    {
        CreateMap<DimLocation, DimLocationDto>();
        CreateMap<CreateDimLocationDto, DimLocation>();
        CreateMap<UpdateDimLocationDto, DimLocation>();
    }
}