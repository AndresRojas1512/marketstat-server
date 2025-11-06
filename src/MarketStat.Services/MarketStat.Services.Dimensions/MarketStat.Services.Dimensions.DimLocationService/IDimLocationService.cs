using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimLocationService;

public interface IDimLocationService
{
    Task<DimLocation> CreateLocationAsync(string cityName, string oblastName, string distrctName);
    Task<DimLocation> GetLocationByIdAsync(int locationId);
    Task<IEnumerable<DimLocation>> GetAllLocationsAsync();
    Task<DimLocation> UpdateLocationAsync(int locationId, string cityName, string oblastName, string districtName);
    Task DeleteLocationAsync(int locationId);
    Task<IEnumerable<string>> GetDistinctDistrictsAsync();
    Task<IEnumerable<string>> GetDistinctOblastsAsync(string districtName);
    Task<IEnumerable<string>> GetDistinctCitiesAsync(string oblastName);
}