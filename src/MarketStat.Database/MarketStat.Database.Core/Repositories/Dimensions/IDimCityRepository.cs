using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimCityRepository
{
    Task AddCityAsync(DimCity city);
    Task<DimCity> GetCityByIdAsync(int cityId);
    Task<IEnumerable<DimCity>> GetAllCitiesAsync();
    Task UpdateCityAsync(DimCity city);
    Task DeleteCityAsync(int cityId);
    Task<IEnumerable<DimCity>> GetCitiesByOblastIdAsync(int oblastId);
}