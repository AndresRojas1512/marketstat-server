using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimLocationRepository : BaseRepository, IDimLocationRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimLocationRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddLocationAsync(DimLocation location)
    {
        var dbModel = DimLocationConverter.ToDbModel(location);
        await _dbContext.DimLocations.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"A location with city '{location.CityName}, Oblast '{location.OblastName}', District '{location.DistrictName}' already exists.");
        }
        location.LocationId = dbModel.LocationId;
    }

    public async Task<DimLocation> GetLocationByIdAsync(int locationId)
    {
        var dbLocation = await _dbContext.DimLocations.FindAsync(locationId);
        if (dbLocation is null)
            throw new NotFoundException($"Location with ID {locationId} not found.");
        return DimLocationConverter.ToDomain(dbLocation);
    }

    public async Task<IEnumerable<DimLocation>> GetAllLocationsAsync()
    {
        var allDbLocations = await _dbContext.DimLocations.AsNoTracking().OrderBy(l => l.DistrictName)
            .ThenBy(l => l.OblastName).ThenBy(l => l.CityName).ToListAsync();
        return allDbLocations.Select(DimLocationConverter.ToDomain);
    }

    public async Task UpdateLocationAsync(DimLocation location)
    {
        var dbLocation = await _dbContext.DimLocations.FindAsync(location.LocationId);
        if (dbLocation is null)
            throw new NotFoundException($"Location with ID {location.LocationId} not found.");
        
        dbLocation.CityName = location.CityName;
        dbLocation.OblastName = location.OblastName;
        dbLocation.DistrictName = location.DistrictName;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"Updating resulted in a conflict. A location with City '{location.CityName}', Oblast '{location.OblastName}', District '{location.DistrictName}' already exists.");
        }
    }

    public async Task DeleteLocationAsync(int locationId)
    {
        var dbLocation = await _dbContext.DimLocations.FindAsync(locationId);
        if (dbLocation is null)
            throw new NotFoundException($"Location with ID {locationId} not found.");
        _dbContext.DimLocations.Remove(dbLocation);
        await _dbContext.SaveChangesAsync();
    }
}