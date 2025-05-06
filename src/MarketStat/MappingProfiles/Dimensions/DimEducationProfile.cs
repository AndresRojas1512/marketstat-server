using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimEducationProfile : Profile
{
    public DimEducationProfile()
    {
        CreateMap<DimEducation, DimEducationDto>();
        
        CreateMap<CreateDimEducationDto, DimEducation>();

        CreateMap<UpdateDimEducationDto, DimEducation>()
            .ForMember(dest => dest.EducationId, opt => opt.Ignore());
    }
}