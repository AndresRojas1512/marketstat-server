using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimCity;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimCityProfile : Profile
{
    public DimCityProfile()
    {
        CreateMap<DimCity, DimCityDto>();

        CreateMap<CreateDimCityDto, DimCity>();

        CreateMap<UpdateDimCityDto, DimCity>()
            .ForMember(dest => dest.CityId, opt => opt.Ignore());
    }
}