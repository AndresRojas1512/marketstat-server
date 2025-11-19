using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimEmployerQuery
{
    public async Task<DimEmployerDto> GetEmployerById(int id, [Service] IDimEmployerService employerService,
        [Service] IMapper mapper)
    {
        var domainResult = await employerService.GetEmployerByIdAsync(id);
        return mapper.Map<DimEmployerDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimEmployerDto>> GetAllEmployers([Service] IDimEmployerService employerService,
        [Service] IMapper mapper)
    {
        var domainResult = await employerService.GetAllEmployersAsync();
        return mapper.Map<IEnumerable<DimEmployerDto>>(domainResult);
    }
}