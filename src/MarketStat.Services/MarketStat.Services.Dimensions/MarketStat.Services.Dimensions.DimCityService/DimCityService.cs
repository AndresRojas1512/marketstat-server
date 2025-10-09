using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimCityService;

public class DimCityService : IDimCityService
{
    private readonly IDimCityRepository _dimCityRepository;
    private readonly ILogger<DimCityService> _logger;

    public DimCityService(IDimCityRepository dimCityRepository, ILogger<DimCityService> logger)
    {
        _dimCityRepository = dimCityRepository;
        _logger = logger;
    }
    
    public async Task<DimCity> CreateCityAsync(string cityName, int oblastId)
    {
        DimCityValidator.ValidateForCreate(cityName, oblastId);
        var existingCitiesInOblast = await _dimCityRepository.GetCitiesByOblastIdAsync(oblastId);
        if (existingCitiesInOblast.Any(c => c.CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A city named '{cityName}' already exists in this oblast.");
        }
        var city = new DimCity(0, cityName, oblastId);

        try
        {
            await _dimCityRepository.AddCityAsync(city);
            _logger.LogInformation("Created city {CityId} ('{CityName}')", city.CityId, cityName);
            return city;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating city '{CityName}'", cityName);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Cannot create city '{CityName}': oblast {OblastId} not found", cityName, oblastId);
            throw;
        }
    }
    
    public async Task<DimCity> GetCityByIdAsync(int cityId)
    {
        try
        {
            return await _dimCityRepository.GetCityByIdAsync(cityId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "City {CityId} not found", cityId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DimCity>> GetAllCitiesAsync()
    {
        var cities = await _dimCityRepository.GetAllCitiesAsync();
        _logger.LogInformation("Fetched {Count} city records", cities.Count());
        return cities;
    }
    
    public async Task<IEnumerable<DimCity>> GetCitiesByOblastIdAsync(int oblastId)
    {
        _logger.LogInformation("Fetching cities for OblastId: {OblastId}", oblastId);
        if (oblastId <= 0)
        {
            _logger.LogWarning("Invalid OblastId provided for GetCitiesByOblastIdAsync: {OblastId}. Returning empty list.", oblastId);
            throw new ArgumentException("OblastId must be a positive integer.");
        }
        var cities = await _dimCityRepository.GetCitiesByOblastIdAsync(oblastId);
        _logger.LogInformation("Fetched {Count} cities for OblastId: {OblastId}", cities.Count(), oblastId);
        return cities;
    }
    
    public async Task<DimCity> UpdateCityAsync(int cityId, string cityName, int oblastId)
    {
        DimCityValidator.ValidateForUpdate(cityId, cityName, oblastId);
        try
        {
            var existing = await _dimCityRepository.GetCityByIdAsync(cityId);

            existing.CityName = cityName;
            existing.OblastId = oblastId;

            await _dimCityRepository.UpdateCityAsync(existing);
            _logger.LogInformation("Updated city {CityId}", cityId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update city {CityId} not found", cityId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Cannot update city {CityId}: duplicate", cityId);
            throw;
        }
    }
    
    public async Task DeleteCityAsync(int cityId)
    {
        try
        {
            await _dimCityRepository.DeleteCityAsync(cityId);
            _logger.LogInformation("Deleted city {CityId}", cityId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete city {CityId}: not found", cityId);
            throw;
        }
    }
}