namespace MarketStat.MappingProfiles.Dimensions;

using AutoMapper;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.Dimensions;

public class DimEmployerProfile : Profile
{
    public DimEmployerProfile()
    {
        CreateMap<DimEmployer, DimEmployerDto>()
            .ForMember(dest => dest.IndustryField, opt => opt.MapFrom(src => src.DimIndustryField));

        CreateMap<CreateDimEmployerDto, DimEmployer>()
            .ForMember(dest => dest.EmployerId, opt => opt.Ignore())
            .ForMember(dest => dest.FactSalaries, opt => opt.Ignore());

        CreateMap<UpdateDimEmployerDto, DimEmployer>()
            .ForMember(dest => dest.EmployerId, opt => opt.Ignore())
            .ForMember(dest => dest.FactSalaries, opt => opt.Ignore());
    }
}
