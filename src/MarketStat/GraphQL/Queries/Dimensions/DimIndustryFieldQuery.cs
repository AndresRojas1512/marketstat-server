using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Services.Dimensions.DimIndustryFieldService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimIndustryFieldQuery
{
    public async Task<DimIndustryFieldDto> GetIndustryFieldById(int id,
        [Service] IDimIndustryFieldService industryFieldService, [Service] IMapper mapper)
    {
        var domainResult = await industryFieldService.GetIndustryFieldByIdAsync(id);
        return mapper.Map<DimIndustryFieldDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimIndustryFieldDto>> GetAllIndustryFields(
        [Service] IDimIndustryFieldService industryFieldService, [Service] IMapper mapper)
    {
        var domainResult = await industryFieldService.GetAllIndustryFieldsAsync();
        return mapper.Map<IEnumerable<DimIndustryFieldDto>>(domainResult);
    }
}