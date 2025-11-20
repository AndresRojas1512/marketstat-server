using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;
using MarketStat.Services.Dimensions.DimLocationService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimLocationQuery
{
    public async Task<DimLocationDto> GetLocationById(int id, [Service] IDimLocationService locationService,
        [Service] IMapper mapper)
    {
        var domainResult = await locationService.GetLocationByIdAsync(id);
        return mapper.Map<DimLocationDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimLocationDto>> GetAllLocations([Service] IDimLocationService locationService,
        [Service] IMapper mapper)
    {
        var domainResult = await locationService.GetAllLocationsAsync();
        return mapper.Map<IEnumerable<DimLocationDto>>(domainResult);
    }

    public async Task<IEnumerable<string>> GetDistricts([Service] IDimLocationService locationService)
    {
        return await locationService.GetDistinctDistrictsAsync();
    }

    public async Task<IEnumerable<string>> GetOblasts(string districtName,
        [Service] IDimLocationService locationService)
    {
        return await locationService.GetDistinctOblastsAsync(districtName);
    }

    public async Task<IEnumerable<string>> GetCities(string oblastName, [Service] IDimLocationService locationService)
    {
        return await locationService.GetDistinctCitiesAsync(oblastName);
    }
}