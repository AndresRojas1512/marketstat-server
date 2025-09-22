using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        var dbModel = DimCityConverter.ToDbModel(city);
        await _dbContext.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is Npgsql.PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A city named '{city.CityName}' already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException($"Oblast with ID {city.OblastId} was not found.");
        }
        city.CityId = dbModel.CityId;
    }

    public async Task<DimCity> GetCityByIdAsync(int cityId)
    {
        var dbModel = await _dbContext.DimCities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CityId == cityId);
        if (dbModel == null)
            throw new NotFoundException($"City with ID {cityId} not found");
        return DimCityConverter.ToDomain(dbModel);
    }

    public async Task<IEnumerable<DimCity>> GetAllCitiesAsync()
    {
        var allDbModels = await _dbContext.DimCities
            .AsNoTracking()
            .OrderBy(c => c.CityName)
            .ToListAsync();
        return allDbModels.Select(DimCityConverter.ToDomain);
    }
    
    public async Task<IEnumerable<DimCity>> GetCitiesByOblastIdAsync(int oblastId)
    {
        var dbCities = await _dbContext.DimCities
            .Where(c => c.OblastId == oblastId)
            .AsNoTracking()
            .OrderBy(c => c.CityName)
            .ToListAsync();
        return dbCities.Select(DimCityConverter.ToDomain);
    }

    public async Task UpdateCityAsync(DimCity city)
    {
        var dbCity = await _dbContext.DimCities.FindAsync(city.CityId);
        if (dbCity == null)
            throw new NotFoundException($"City with ID {city.CityId} not found");
        
        dbCity.CityName = city.CityName;
        dbCity.OblastId = city.OblastId;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is Npgsql.PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A city named '{city.CityName}' already exists.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException($"Oblast with ID {city.OblastId} not found.");
        }
    }

    public async Task DeleteCityAsync(int cityId)
    {
        var dbModel = await _dbContext.DimCities.FindAsync(cityId);
        if (dbModel is null)
            throw new NotFoundException($"City with ID {cityId} not found.");
        _dbContext.DimCities.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }
}