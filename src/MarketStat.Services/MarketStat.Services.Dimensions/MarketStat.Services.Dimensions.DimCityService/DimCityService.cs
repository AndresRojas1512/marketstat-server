using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
        var city = new DimCity(0, cityName, oblastId);

        try
        {
            await _dimCityRepository.AddCityAsync(city);
            _logger.LogInformation("Created DimCity {CityId}", city.CityId);
            return city;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimCity (duplicate {CityId})", city.CityId);
            throw new Exception($"A city with ID {city.CityId} already exists.");
        }
    }
    
    public async Task<DimCity> GetCityByIdAsync(int cityId)
    {
        try
        {
            return await _dimCityRepository.GetCityByIdAsync(cityId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "City {CityId} not found", cityId);
            throw new Exception($"City with ID {cityId} was not found.");
        }
    }
    
    public async Task<IEnumerable<DimCity>> GetAllCitiesAsync()
    {
        var cities = await _dimCityRepository.GetAllCitiesAsync();
        _logger.LogInformation("Fetched {Count} city records", cities.Count());
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
            _logger.LogInformation("Updated DimCity {CityId}", cityId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update – City {CityId} not found", cityId);
            throw new Exception($"Cannot update: city {cityId} was not found.");
        }
    }
    
    public async Task DeleteCityAsync(int cityId)
    {
        try
        {
            await _dimCityRepository.DeleteCityAsync(cityId);
            _logger.LogInformation("Deleted DimCity {CityId}", cityId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete – City {CityId} not found", cityId);
            throw new Exception($"Cannot delete: city {cityId} not found.");
        }
    }
}