using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimFederalDistrictService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimFederalDistrictService;

public class DimFederalDistrictService : IDimFederalDistrictService
{
    private readonly IDimFederalDistrictRepository _dimFederalDistrictRepository;
    private readonly ILogger<DimFederalDistrictService> _logger;

    public DimFederalDistrictService(IDimFederalDistrictRepository dimFederalDistrictRepository,
        ILogger<DimFederalDistrictService> logger)
    {
        _dimFederalDistrictRepository = dimFederalDistrictRepository;
        _logger = logger;
    }
    
    public async Task<DimFederalDistrict> CreateDistrictAsync(string districtName)
    {
        var allDistricts = (await _dimFederalDistrictRepository.GetAllFederalDistrictsAsync()).ToList();
        int newId = allDistricts.Any() ? allDistricts.Max(d => d.DistrictId) + 1 : 1;
        DimFederalDistrictValidator.ValidateParameters(newId, districtName);
        var district = new DimFederalDistrict(newId, districtName);

        try
        {
            await _dimFederalDistrictRepository.AddFederalDistrictAsync(district);
            _logger.LogInformation("Created District {DistrictId}", newId);
            return district;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimDistrict {DistrictId}.", district.DistrictId);
            throw new Exception($"An employer with ID {district.DistrictId} already exists.");
        }
    }

    public async Task<DimFederalDistrict> GetDistrictByIdAsync(int id)
    {
        try
        {
            return await _dimFederalDistrictRepository.GetFederalDistrictByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "District {DistrictId} not found", id);
            throw new Exception($"District with ID {id} was not found.");
        }
    }

    public async Task<IEnumerable<DimFederalDistrict>> GetAllDistrictsAsync()
    {
        var districts = await _dimFederalDistrictRepository.GetAllFederalDistrictsAsync();
        _logger.LogInformation("Fetched {Count} District records", districts.Count());
        return districts;
    }

    public async Task<DimFederalDistrict> UpdateDistrictAsync(int districtId, string districtName)
    {
        DimFederalDistrictValidator.ValidateParameters(districtId, districtName);
        try
        {
            var existing = await _dimFederalDistrictRepository.GetFederalDistrictByIdAsync(districtId);
            existing.DistrictName = districtName;
            await _dimFederalDistrictRepository.UpdateFederalDistrictAsync(existing);
            _logger.LogInformation("Updated DimFederalDistrict {DistrictId}", districtId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update - District {DistrictId} not found", districtId);
            throw new Exception($"Cannot update: district {districtId} was not found.");
        }
    }

    public async Task DeleteDistrictAsync(int id)
    {
        try
        {
            await _dimFederalDistrictRepository.DeleteFederalDistrictAsync(id);
            _logger.LogInformation("Deleted DimFederalDistrict {DistrictId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete, DimFederalDistrict {DistrictId} not found", id);
            throw new Exception($"Cannot delete: district {id} not found.");
        }
    }
}