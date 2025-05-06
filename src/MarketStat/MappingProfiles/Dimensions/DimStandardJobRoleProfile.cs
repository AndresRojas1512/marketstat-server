using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRole;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimStandardJobRoleProfile : Profile
{
    public DimStandardJobRoleProfile()
    {
        CreateMap<DimStandardJobRole, DimStandardJobRoleDto>();
        
        CreateMap<CreateDimStandardJobRoleDto, DimStandardJobRole>();
        
        CreateMap<UpdateDimStandardJobRoleDto, DimStandardJobRole>()
            .ForMember(dest => dest.StandardJobRoleId, opt => opt.Ignore());
    }
}