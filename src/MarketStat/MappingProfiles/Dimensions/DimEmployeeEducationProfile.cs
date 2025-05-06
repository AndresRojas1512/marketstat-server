using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployeeEducation;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEmployeeEducationProfile : Profile
{
    public DimEmployeeEducationProfile()
    {
        CreateMap<DimEmployeeEducation, DimEmployeeEducationDto>();
        
        CreateMap<CreateDimEmployeeEducationDto, DimEmployeeEducation>();
        
        CreateMap<UpdateDimEmployeeEducationDto, DimEmployeeEducation>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.EducationId, opt => opt.Ignore());
    }
}