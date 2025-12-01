namespace MarketStat.MappingProfiles.Dimensions;

using AutoMapper;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.Dimensions.DimIndustryField;

public class DimIndustryFieldProfile : Profile
{
    public DimIndustryFieldProfile()
    {
        CreateMap<DimIndustryField, DimIndustryFieldDto>();

        CreateMap<CreateDimIndustryFieldDto, DimIndustryField>()
            .ForMember(dest => dest.IndustryFieldId, opt => opt.Ignore());

        CreateMap<UpdateDimIndustryFieldDto, DimIndustryField>()
            .ForMember(dest => dest.IndustryFieldId, opt => opt.Ignore());
    }
}
