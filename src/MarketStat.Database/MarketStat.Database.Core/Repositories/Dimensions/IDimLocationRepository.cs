using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimLocationRepository
{
    Task AddLocationAsync(DimLocation location);
    Task<DimLocation> GetLocationByIdAsync(int locationId);
    Task<IEnumerable<DimLocation>> GetAllLocationsAsync();
    Task UpdateLocationAsync(DimLocation location);
    Task DeleteLocationAsync(int locationId);
}