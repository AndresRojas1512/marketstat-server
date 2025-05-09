using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
        DimFederalDistrictValidator.ValidateForCreate(districtName);
        var district = new DimFederalDistrict(0, districtName);

        try
        {
            await _dimFederalDistrictRepository.AddFederalDistrictAsync(district);
            _logger.LogInformation("Created district {DistrictId} with name {DistrictName}", district.DistrictId, district.DistrictName);
            return district;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating district {DistrictId}.", district.DistrictId);
            throw;
        }
    }

    public async Task<DimFederalDistrict> GetDistrictByIdAsync(int id)
    {
        try
        {
            return await _dimFederalDistrictRepository.GetFederalDistrictByIdAsync(id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "District {DistrictId} not found", id);
            throw;
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
        DimFederalDistrictValidator.ValidateForUpdate(districtId, districtName);
        try
        {
            var existing = await _dimFederalDistrictRepository.GetFederalDistrictByIdAsync(districtId);
            existing.DistrictName = districtName;
            await _dimFederalDistrictRepository.UpdateFederalDistrictAsync(existing);
            _logger.LogInformation("Updated DimFederalDistrict {DistrictId}", districtId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: district {DistrictId} not found", districtId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict updating federal district {DistrictId}", districtId);
            throw;
        }
    }

    public async Task DeleteDistrictAsync(int id)
    {
        try
        {
            await _dimFederalDistrictRepository.DeleteFederalDistrictAsync(id);
            _logger.LogInformation("Deleted DimFederalDistrict {DistrictId}", id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete: federal district {DistrictId} not found", id);
            throw;
        }
    }
}