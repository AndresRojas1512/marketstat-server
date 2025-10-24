using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;

namespace MarketStat.MappingProfiles.Dimensions;

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