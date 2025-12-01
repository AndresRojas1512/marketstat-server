namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimLocationRepository
{
    Task AddLocationAsync(DimLocation location);

    Task<DimLocation> GetLocationByIdAsync(int locationId);

    Task<IEnumerable<DimLocation>> GetAllLocationsAsync();

    Task UpdateLocationAsync(DimLocation location);

    Task DeleteLocationAsync(int locationId);

    Task<IEnumerable<string>> GetDistinctDistrictsAsync();

    Task<IEnumerable<string>> GetDistinctOblastsAsync(string districtName);

    Task<IEnumerable<string>> GetDistinctCitiesAsync(string oblastName);

    Task<List<int>> GetLocationIdsByFilterAsync(string? districtName, string? oblastName, string? cityName);
}
