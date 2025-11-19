using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;
using MarketStat.Services.Dimensions.DimDateService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimDateQuery
{
    public async Task<DimDateDto> GetDateById(int id, [Service] IDimDateService dateService, [Service] IMapper mapper)
    {
        var domainResult = await dateService.GetDateByIdAsync(id);
        return mapper.Map<DimDateDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimDateDto>> GetAllDates([Service] IDimDateService dateService,
        [Service] IMapper mapper)
    {
        var domainResult = await dateService.GetAllDatesAsync();
        return mapper.Map<IEnumerable<DimDateDto>>(domainResult);
    }
}