using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimOblastService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimOblastService;

public class DimOblastService : IDimOblastService
{
    private readonly IDimOblastRepository _dimOblastRepository;
    private readonly ILogger<IDimOblastService> _logger;

    public DimOblastService(IDimOblastRepository dimOblastRepository, ILogger<IDimOblastService> logger)
    {
        _dimOblastRepository = dimOblastRepository;
        _logger = logger;
    }
    
    public async Task<DimOblast> CreateOblastAsync(string oblastName, int districtId)
    {
        DimOblastValidator.ValidateForCreate(oblastName, districtId);
        var oblast = new DimOblast(0, oblastName, districtId);

        try
        {
            await _dimOblastRepository.AddOblastAsync(oblast);
            _logger.LogInformation("Created DimOblast {OblastId}", oblast.OblastId);
            return oblast;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimIndustryField {IndustryFieldId}.", oblast.OblastId);
            throw new Exception($"Could not create oblast {oblastName}");
        }
    }

    public async Task<DimOblast> GetOblastByIdAsync(int oblastId)
    {
        try
        {
            return await _dimOblastRepository.GetOblastByIdAsync(oblastId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Oblast {OblastId} not found", oblastId);
            throw new Exception($"Oblast {oblastId} was not found.");
        }
    }

    public async Task<IEnumerable<DimOblast>> GetAllOblastsAsync()
    {
        var oblasts = await _dimOblastRepository.GetAllOblastsAsync();
        _logger.LogInformation("Fetched {Count} oblasts", oblasts.Count());
        return oblasts;
    }

    public async Task<IEnumerable<DimOblast>> GetOblastsByFederalDistrictIdAsync(int districtId)
    {
        try
        {
            var oblasts = await _dimOblastRepository.GetOblastsByFederalDistrictIdAsync(districtId);
            _logger.LogInformation("Fetched {Count} oblast(s) for federal distric {DistrictId}.", oblasts.Count(),
                districtId);
            return oblasts;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching oblasts for federal district {DistrictId}", districtId);
            throw new Exception($"Could not retrieve oblasts for district {districtId}.");
        }
    }

    public async Task<DimOblast> UpdateOblastAsync(int oblastId, string oblastName, int districtId)
    {
        DimOblastValidator.ValidateForUpdate(oblastId, oblastName, districtId);
        try
        {
            var existing = await _dimOblastRepository.GetOblastByIdAsync(oblastId);
            existing.OblastName = oblastName;
            existing.DistrictId = districtId;
            await _dimOblastRepository.UpdateOblastAsync(existing);
            _logger.LogInformation("Updated Oblast {OblastId}", oblastId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update oblast {OblastId}", oblastId);
            throw new Exception($"Cannot update: oblast {oblastId} not found.");
        }
    }

    public async Task DeleteOblastAsync(int id)
    {
        try
        {
            await _dimOblastRepository.DeleteOblastAsync(id);
            _logger.LogInformation("Deleted Oblast {OblastId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete oblast {OblastId}", id);
            throw new Exception($"Cannot delete: oblast {id} not found.");
        }
    }
}