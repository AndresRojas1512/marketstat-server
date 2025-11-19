using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimEducation;
using MarketStat.Services.Dimensions.DimEducationService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimEducationQuery
{
    public async Task<DimEducationDto> GetEducationById(int id, [Service] IDimEducationService educationService,
        [Service] IMapper mapper)
    {
        var domainResult = await educationService.GetEducationByIdAsync(id);
        return mapper.Map<DimEducationDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimEducationDto>> GetAllEducations([Service] IDimEducationService educationService,
        [Service] IMapper mapper)
    {
        var domainResult = await educationService.GetAllEducationsAsync();
        return mapper.Map<IEnumerable<DimEducationDto>>(domainResult);
    }
}