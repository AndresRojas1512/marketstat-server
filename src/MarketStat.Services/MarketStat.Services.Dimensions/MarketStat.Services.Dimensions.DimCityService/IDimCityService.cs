using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimCityService;

public interface IDimCityService
{
    Task<DimCity> CreateCityAsync(string cityName, string oblastName, string federalDistrict);
    Task<DimCity> GetCityByIdAsync(int cityId);
    Task<IEnumerable<DimCity>> GetAllCitiesAsync();
    Task<DimCity> UpdateCityAsync(int cityId, string cityName, string oblastName, string federalDistrict);
    Task DeleteCityAsync(int cityId);
}