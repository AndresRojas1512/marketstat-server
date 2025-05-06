using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimHierarchyLevelProfile : Profile
{
    public DimHierarchyLevelProfile()
    {
        CreateMap<DimHierarchyLevel, DimHierarchyLevelDto>();
        
        CreateMap<CreateDimHierarchyLevelDto, DimHierarchyLevel>();
        
        CreateMap<UpdateDimHierarchyLevelDto, DimHierarchyLevel>()
            .ForMember(dest => dest.HierarchyLevelId, opt => opt.Ignore());
    }
}