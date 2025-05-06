using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJobRole;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimJobRoleProfile : Profile
{
    public DimJobRoleProfile()
    {
        CreateMap<DimJobRole, DimJobRoleDto>();
        
        CreateMap<CreateDimJobRoleDto, DimJobRole>();

        CreateMap<UpdateDimJobRoleDto, DimJobRole>()
            .ForMember(dest => dest.JobRoleId, opt => opt.Ignore());
    }
}