using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimOblast;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimOblastProfile : Profile
{
    public DimOblastProfile()
    {
        CreateMap<DimOblast, DimOblastDto>();
        
        CreateMap<CreateDimOblastDto, DimOblast>();
        
        CreateMap<UpdateDimOblastDto, DimOblast>()
            .ForMember(dest => dest.OblastId, opt => opt.Ignore());
    }
}