namespace MarketStat.MappingProfiles.Dimensions;

using AutoMapper;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.Dimensions.DimLocation;

public class DimLocationProfile : Profile
{
    public DimLocationProfile()
    {
        CreateMap<DimLocation, DimLocationDto>();
        CreateMap<CreateDimLocationDto, DimLocation>();
        CreateMap<UpdateDimLocationDto, DimLocation>();
    }
}
