namespace MarketStat.MappingProfiles.Dimensions;

using AutoMapper;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.Dimensions.DimEducation;

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
