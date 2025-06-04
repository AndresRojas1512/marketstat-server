using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEmployerProfile : Profile
{
    public DimEmployerProfile()
    {
        CreateMap<DimEmployer, DimEmployerDto>();

        CreateMap<CreateDimEmployerDto, DimEmployer>()
            .ForMember(dest => dest.EmployerId, opt => opt.Ignore())
            .ForMember(dest => dest.EmployerIndustryFields, opt => opt.Ignore())
            .ForMember(dest => dest.FactSalaries, opt => opt.Ignore());

        CreateMap<UpdateDimEmployerDto, DimEmployer>()
            .ForMember(dest => dest.EmployerId, opt => opt.Ignore()) // ID is typically from the route/existing entity, not DTO
            .ForMember(dest => dest.EmployerIndustryFields, opt => opt.Ignore())
            .ForMember(dest => dest.FactSalaries, opt => opt.Ignore());
    }
}