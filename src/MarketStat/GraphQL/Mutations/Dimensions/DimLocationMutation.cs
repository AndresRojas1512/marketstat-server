using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;
using MarketStat.Services.Dimensions.DimLocationService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimLocationMutation
{
    public async Task<DimLocationDto> CreateLocation(CreateDimLocationDto input,
        [Service] IDimLocationService locationService, [Service] IMapper mapper)
    {
        var createdDomain =
            await locationService.CreateLocationAsync(input.CityName, input.OblastName, input.DistrictName);
        return mapper.Map<DimLocationDto>(createdDomain);
    }

    public async Task<DimLocationDto> UpdateLocation(int id, UpdateDimLocationDto input,
        [Service] IDimLocationService locationService, [Service] IMapper mapper)
    {
        var updatedDomain =
            await locationService.UpdateLocationAsync(id, input.CityName, input.OblastName, input.DistrictName);
        return mapper.Map<DimLocationDto>(updatedDomain);
    }

    public async Task<bool> DeleteLocation(int id, [Service] IDimLocationService locationService)
    {
        await locationService.DeleteLocationAsync(id);
        return true;
    }
}