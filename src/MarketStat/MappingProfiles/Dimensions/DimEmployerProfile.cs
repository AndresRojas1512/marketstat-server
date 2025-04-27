using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEmployerProfile : Profile
{
    public DimEmployerProfile()
    {
        CreateMap<DimEmployer, DimEmployerDto>();
    }
}