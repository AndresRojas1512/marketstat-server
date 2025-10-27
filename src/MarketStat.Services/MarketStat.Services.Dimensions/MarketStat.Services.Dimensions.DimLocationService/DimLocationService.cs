using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimencions.DimLocationService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimencions.DimLocationService;

public class DimLocationService : IDimLocationService
{
    private readonly IDimLocationRepository _dimLocationRepository;
    private readonly ILogger<DimLocationService> _logger;

    public DimLocationService(IDimLocationRepository dimLocationRepository, ILogger<DimLocationService> logger)
    {
        _dimLocationRepository = dimLocationRepository;
        _logger = logger;
    }
    
    public async Task<DimLocation> CreateLocationAsync(string cityName, string oblastName, string distrctName)
    {
        DimLocationValidator.ValidateForCreate(cityName, oblastName, distrctName);
        var location = new DimLocation(0, cityName, oblastName, distrctName);
        try
        {
            await _dimLocationRepository.AddLocationAsync(location);
            _logger.LogInformation("Created location {LocationId}", location.LocationId);
            return location;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating location.");
            throw;
        }
    }

    public async Task<DimLocation> GetLocationByIdAsync(int locationId)
    {
        try
        {
            return await _dimLocationRepository.GetLocationByIdAsync(locationId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Location {Id} not found.", locationId);
            throw;
        }
    }

    public async Task<IEnumerable<DimLocation>> GetAllLocationsAsync()
    {
        var list = await _dimLocationRepository.GetAllLocationsAsync();
        _logger.LogInformation("Fetched {Count} location records", list.Count());
        return list;
    }

    public async Task<DimLocation> UpdateLocationAsync(int locationId, string cityName, string oblastName,
        string districtName)
    {
        DimLocationValidator.ValidateForUpdate(locationId, cityName, oblastName, districtName);
        try
        {
            var existing = await _dimLocationRepository.GetLocationByIdAsync(locationId);

            existing.CityName = cityName;
            existing.OblastName = oblastName;
            existing.DistrictName = districtName;

            await _dimLocationRepository.UpdateLocationAsync(existing);
            _logger.LogInformation("Updated location {Id}", locationId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: location {Id} not found", locationId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict update location {Id}", locationId);
            throw;
        }
    }

    public async Task DeleteLocationAsync(int locationId)
    {
        try
        {
            await _dimLocationRepository.DeleteLocationAsync(locationId);
            _logger.LogInformation("Deleted location {LocationId}", locationId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete: Location {Id} not found", locationId);
            throw;
        }
    }
}