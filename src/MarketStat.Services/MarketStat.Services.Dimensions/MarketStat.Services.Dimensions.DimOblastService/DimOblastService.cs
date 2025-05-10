using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating oblast {OblastId}.", oblast.OblastId);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "FKs not found: Federal district");
            throw;
        }
    }

    public async Task<DimOblast> GetOblastByIdAsync(int oblastId)
    {
        try
        {
            return await _dimOblastRepository.GetOblastByIdAsync(oblastId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Oblast {OblastId} not found", oblastId);
            throw;
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
        var oblasts = await _dimOblastRepository.GetOblastsByFederalDistrictIdAsync(districtId);
        _logger.LogInformation("Fetched {Count} oblast(s) for federal distric {DistrictId}.", oblasts.Count(),
            districtId);
        return oblasts;
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update oblast {OblastId}", oblastId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict updating oblast {OblastId}, already exists", oblastId);
            throw;
        }
    }

    public async Task DeleteOblastAsync(int id)
    {
        try
        {
            await _dimOblastRepository.DeleteOblastAsync(id);
            _logger.LogInformation("Deleted Oblast {OblastId}", id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete oblast {OblastId}", id);
            throw;
        }
    }
}