using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimCityRepository : BaseRepository, IDimCityRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimCityRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddCityAsync(DimCity city)
    {
        var dbCity = DimCityConverter.ToDbModel(city);
        await _dbContext.AddAsync(dbCity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimCity> GetCityByIdAsync(int cityId)
    {
        var dbCity = await _dbContext.DimCities.FindAsync(cityId) 
                     ?? throw new KeyNotFoundException($"City with id {cityId} not found");
        return DimCityConverter.ToDomain(dbCity);
    }

    public async Task<IEnumerable<DimCity>> GetAllCitiesAsync()
    {
        var dbAllCities = await _dbContext.DimCities.ToListAsync();
        return dbAllCities.Select(DimCityConverter.ToDomain);
    }

    public async Task UpdateCityAsync(DimCity city)
    {
        var dbCity = await _dbContext.DimCities.FindAsync(city.CityId) 
                     ?? throw new KeyNotFoundException($"Cannot update: City with id {city.CityId} not found");
        dbCity.CityName = city.CityName;
        dbCity.OblastId = city.OblastId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCityAsync(int cityId)
    {
        var dbCity = await _dbContext.DimCities.FindAsync(cityId) 
                     ?? throw new KeyNotFoundException($"Cannot delete: City with id {cityId} not found");
        _dbContext.DimCities.Remove(dbCity);
        await _dbContext.SaveChangesAsync();
    }
}