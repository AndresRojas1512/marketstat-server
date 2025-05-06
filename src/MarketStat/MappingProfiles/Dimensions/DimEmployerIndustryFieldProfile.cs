using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEmployerIndustryField;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEmployerIndustryFieldProfile : Profile
{
    public DimEmployerIndustryFieldProfile()
    {
        CreateMap<DimEmployerIndustryField, DimEmployerIndustryFieldDto>();
        
        CreateMap<CreateDimEmployerIndustryFieldDto, DimEmployerIndustryField>();
        
        CreateMap<UpdateDimEmployerIndustryFieldDto, DimEmployerIndustryField>()
            .ForMember(dest => dest.EmployerId, opt => opt.Ignore())
            .ForMember(dest => dest.IndustryFieldId, opt => opt.Ignore());
    }
}