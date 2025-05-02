using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimCityService;

public interface IDimCityService
{
    Task<DimCity> CreateCityAsync(string cityName, int oblastId);
    Task<DimCity> GetCityByIdAsync(int cityId);
    Task<IEnumerable<DimCity>> GetAllCitiesAsync();
    Task<DimCity> UpdateCityAsync(int cityId, string cityName, int oblastId);
    Task DeleteCityAsync(int cityId);
}