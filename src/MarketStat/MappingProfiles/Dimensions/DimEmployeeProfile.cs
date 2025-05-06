using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEmployeeProfile : Profile
{
    public DimEmployeeProfile()
    {
        CreateMap<DimEmployee, DimEmployeeDto>();

        CreateMap<CreateDimEmployeeDto, DimEmployee>();
        
        CreateMap<UpdateDimEmployeeDto, DimEmployee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore());
    }
}