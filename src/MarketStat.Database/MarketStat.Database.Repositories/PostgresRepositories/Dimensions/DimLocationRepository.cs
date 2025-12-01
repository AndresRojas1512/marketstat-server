namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

using MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class DimLocationRepository : BaseRepository, IDimLocationRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimLocationRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddLocationAsync(DimLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        var dbModel = DimLocationConverter.ToDbModel(location);
        await _dbContext.DimLocations.AddAsync(dbModel).ConfigureAwait(false);
        try
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
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
        var dbLocation = await _dbContext.DimLocations.FindAsync(locationId).ConfigureAwait(false);
        if (dbLocation is null)
        {
            throw new NotFoundException($"Location with ID {locationId} not found.");
        }

        return DimLocationConverter.ToDomain(dbLocation);
    }

    public async Task<IEnumerable<DimLocation>> GetAllLocationsAsync()
    {
        var allDbLocations = await _dbContext.DimLocations.AsNoTracking().OrderBy(l => l.DistrictName)
            .ThenBy(l => l.OblastName).ThenBy(l => l.CityName).ToListAsync().ConfigureAwait(false);
        return allDbLocations.Select(DimLocationConverter.ToDomain);
    }

    public async Task UpdateLocationAsync(DimLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        var dbLocation = await _dbContext.DimLocations.FindAsync(location.LocationId).ConfigureAwait(false);
        if (dbLocation is null)
        {
            throw new NotFoundException($"Location with ID {location.LocationId} not found.");
        }

        dbLocation.CityName = location.CityName;
        dbLocation.OblastName = location.OblastName;
        dbLocation.DistrictName = location.DistrictName;

        try
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                $"Updating resulted in a conflict. A location with City '{location.CityName}', Oblast '{location.OblastName}', District '{location.DistrictName}' already exists.");
        }
    }

    public async Task DeleteLocationAsync(int locationId)
    {
        var dbLocation = await _dbContext.DimLocations.FindAsync(locationId).ConfigureAwait(false);
        if (dbLocation is null)
        {
            throw new NotFoundException($"Location with ID {locationId} not found.");
        }

        _dbContext.DimLocations.Remove(dbLocation);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetDistinctDistrictsAsync()
    {
        return await _dbContext.DimLocations
            .Select(l => l.DistrictName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetDistinctOblastsAsync(string districtName)
    {
        return await _dbContext.DimLocations
            .Where(l => l.DistrictName == districtName)
            .Select(l => l.OblastName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetDistinctCitiesAsync(string oblastName)
    {
        return await _dbContext.DimLocations
            .Where(l => l.OblastName == oblastName)
            .Select(l => l.CityName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<int>> GetLocationIdsByFilterAsync(string? districtName, string? oblastName, string? cityName)
    {
        var query = _dbContext.DimLocations.AsQueryable();
        if (!string.IsNullOrEmpty(districtName))
        {
            query = query.Where(l => l.DistrictName == districtName);
        }

        if (!string.IsNullOrEmpty(oblastName))
        {
            query = query.Where(l => l.OblastName == oblastName);
        }

        if (!string.IsNullOrEmpty(cityName))
        {
            query = query.Where(l => l.CityName == cityName);
        }

        return await query.Select(l => l.LocationId).ToListAsync().ConfigureAwait(false);
    }
}
