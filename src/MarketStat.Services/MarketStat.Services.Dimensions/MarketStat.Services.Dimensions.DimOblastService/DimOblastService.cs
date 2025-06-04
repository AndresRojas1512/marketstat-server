using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimOblastService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimOblastService;

public class DimOblastService : IDimOblastService
{
    private readonly IDimOblastRepository _dimOblastRepository;
    private readonly ILogger<DimOblastService> _logger;

    public DimOblastService(IDimOblastRepository dimOblastRepository, ILogger<DimOblastService> logger)
    {
        _dimOblastRepository = dimOblastRepository;
        _logger = logger;
    }
    
    public async Task<DimOblast> CreateOblastAsync(string oblastName, int districtId)
    {
        DimOblastValidator.ValidateForCreate(oblastName, districtId);
        _logger.LogInformation("Attempting to create oblast: {OblastName} in DistrictId: {DistrictId}", oblastName, districtId);
        var oblast = new DimOblast(0, oblastName, districtId);

        try
        {
            await _dimOblastRepository.AddOblastAsync(oblast);
            _logger.LogInformation("Created DimOblast {OblastId} ('{OblastName}') in DistrictId {DistrictId}", 
                oblast.OblastId, oblast.OblastName, oblast.DistrictId);
            return oblast;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating oblast '{OblastName}'.", oblastName);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Failed to create oblast '{OblastName}' due to invalid DistrictId {DistrictId}.", oblastName, districtId);
            throw;
        }
    }

    public async Task<DimOblast> GetOblastByIdAsync(int oblastId)
    {
        _logger.LogInformation("Fetching DimOblast by ID: {OblastId}", oblastId);
        return await _dimOblastRepository.GetOblastByIdAsync(oblastId);
    }

    public async Task<IEnumerable<DimOblast>> GetAllOblastsAsync()
    {
        _logger.LogInformation("Fetching all DimOblasts.");
        var oblasts = await _dimOblastRepository.GetAllOblastsAsync();
        _logger.LogInformation("Fetched {Count} oblast records.", oblasts.Count());
        return oblasts;
    }

    public async Task<IEnumerable<DimOblast>> GetOblastsByFederalDistrictIdAsync(int districtId)
    {
        _logger.LogInformation("Fetching oblasts for FederalDistrictId: {DistrictId}", districtId);
        if (districtId <= 0)
        {
            _logger.LogWarning("GetOblastsByFederalDistrictIdAsync called with invalid DistrictId: {DistrictId}. Returning empty list.", districtId);
            return Enumerable.Empty<DimOblast>();
        }
        var oblasts = await _dimOblastRepository.GetOblastsByFederalDistrictIdAsync(districtId);
        _logger.LogInformation("Fetched {Count} oblast(s) for FederalDistrictId: {DistrictId}.", oblasts.Count(), districtId);
        return oblasts;
    }

    public async Task<DimOblast> UpdateOblastAsync(int oblastId, string oblastName, int districtId)
    {
        DimOblastValidator.ValidateForUpdate(oblastId, oblastName, districtId);
        _logger.LogInformation("Attempting to update DimOblast {OblastId}", oblastId);
        try
        {
            var existingOblast = await _dimOblastRepository.GetOblastByIdAsync(oblastId);

            existingOblast.OblastName = oblastName;
            existingOblast.DistrictId = districtId;
            
            await _dimOblastRepository.UpdateOblastAsync(existingOblast);
            _logger.LogInformation("Updated DimOblast {OblastId}", oblastId);
            return existingOblast;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: Oblast {OblastId} or its associated DistrictId not found.", oblastId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict when updating oblast {OblastId}.", oblastId);
            throw;
        }
    }

    public async Task DeleteOblastAsync(int id)
    {
        _logger.LogInformation("Attempting to delete DimOblast {OblastId}", id);
        await _dimOblastRepository.DeleteOblastAsync(id);
        _logger.LogInformation("Deleted DimOblast {OblastId}", id);
    }
}