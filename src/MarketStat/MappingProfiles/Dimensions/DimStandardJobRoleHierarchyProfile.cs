using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRoleHierarchy;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimStandardJobRoleHierarchyProfile : Profile
{
    public DimStandardJobRoleHierarchyProfile()
    {
        CreateMap<DimStandardJobRoleHierarchy, DimStandardJobRoleHierarchyDto>();
        
        CreateMap<CreateDimStandardJobRoleHierarchyDto, DimStandardJobRoleHierarchy>();

        CreateMap<UpdateDimStandardJobRoleHierarchyDto, DimStandardJobRoleHierarchy>()
            .ForMember(dest => dest.StandardJobRoleId, opt => opt.Ignore())
            .ForMember(dest => dest.HierarchyLevelId, opt => opt.Ignore());
    }
}