using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimFederalDistrict;

namespace MarketStat.MappingProfiles.Dimensions;

public class DimFederalDistrictProfile : Profile
{
    public DimFederalDistrictProfile()
    {
        CreateMap<DimFederalDistrict, DimFederalDistrictDto>();
        
        CreateMap<CreateDimFederalDistrictDto, DimFederalDistrict>();

        CreateMap<UpdateDimFederalDistrictDto, DimFederalDistrict>()
            .ForMember(dest => dest.DistrictId, opt => opt.Ignore());
    }
}