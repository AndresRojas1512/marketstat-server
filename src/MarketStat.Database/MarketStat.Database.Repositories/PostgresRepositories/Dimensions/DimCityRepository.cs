using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimCityRepository : IDimCityRepository
{
    private readonly Dictionary<int, DimCity> _cities = new Dictionary<int, DimCity>();
    
    public Task AddCityAsync(DimCity city)
    {
        if (!_cities.TryAdd(city.CityId, city))
        {
            throw new ArgumentException($"City {city.CityId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimCity> GetCityByIdAsync(int cityId)
    {
        if (_cities.TryGetValue(cityId, out var c))
        {
            return Task.FromResult(c);
        }
        throw new KeyNotFoundException($"City {cityId} not found.");
    }

    public Task<IEnumerable<DimCity>> GetAllCitiesAsync()
    {
        return Task.FromResult<IEnumerable<DimCity>>(_cities.Values);
    }

    public Task UpdateCityAsync(DimCity city)
    {
        if (!_cities.ContainsKey(city.CityId))
        {
            throw new KeyNotFoundException($"Cannot update: city {city.CityId} not found.");
        }
        _cities[city.CityId] = city;
        return Task.CompletedTask;
    }

    public Task DeleteCityAsync(int cityId)
    {
        if (!_cities.ContainsKey(cityId))
        {
            throw new KeyNotFoundException($"Cannot delete: city {cityId} not found.");
        }
        return Task.CompletedTask;
    }
}