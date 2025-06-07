using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployee;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEmployeeProfile : Profile
{
    public DimEmployeeProfile()
    {
        CreateMap<DimEmployee, DimEmployeeDto>();

        CreateMap<CreateDimEmployeeDto, DimEmployee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.DimEmployeeEducations, opt => opt.Ignore())
            .ForMember(dest => dest.FactSalaries, opt => opt.Ignore());
            
        CreateMap<UpdateDimEmployeeDto, DimEmployee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.DimEmployeeEducations, opt => opt.Ignore())
            .ForMember(dest => dest.FactSalaries, opt => opt.Ignore());
    }
}