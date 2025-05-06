using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducationLevel;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEducationLevelProfile : Profile
{
    public DimEducationLevelProfile()
    {
        CreateMap<DimEducationLevel, DimEducationLevelDto>();
        
        CreateMap<CreateDimEducationLevelDto, DimEducationLevel>();

        CreateMap<UpdateDimEducationLevelDto, DimEducationLevel>()
            .ForMember(dest => dest.EducationLevelId, opt => opt.Ignore());
    }
}