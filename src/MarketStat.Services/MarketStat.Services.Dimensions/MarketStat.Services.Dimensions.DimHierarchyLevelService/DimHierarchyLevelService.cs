using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimHierarchyLevelService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimHierarchyLevelService;

public class DimHierarchyLevelService : IDimHierarchyLevelService
{
    private readonly IDimHierarchyLevelRepository _dimHierarchyLevelRepository;
    private readonly ILogger<DimHierarchyLevelService> _logger;

    public DimHierarchyLevelService(IDimHierarchyLevelRepository dimHierarchyLevelRepository,
        ILogger<DimHierarchyLevelService> logger)
    {
        _dimHierarchyLevelRepository = dimHierarchyLevelRepository;
        _logger = logger;
    }

    public async Task<DimHierarchyLevel> CreateHierarchyLevelAsync(string hierarchyLevelCode, string hierarchyLevelName)
    {
        DimHierarchyLevelValidator.ValidateForCreate(hierarchyLevelCode, hierarchyLevelName);
        _logger.LogInformation("Service: Attempting to create hierarchy level: {HierarchyLevelName}", hierarchyLevelName);

        var hierarchyLevel = new DimHierarchyLevel(0, hierarchyLevelCode, hierarchyLevelName);
        try
        {
            await _dimHierarchyLevelRepository.AddHierarchyLevelAsync(hierarchyLevel);
            _logger.LogInformation("Service: Created DimHierarchyLevel {HierarchyLevelId} ('{HierarchyLevelName}')", 
                hierarchyLevel.HierarchyLevelId, hierarchyLevel.HierarchyLevelName);
            return hierarchyLevel;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict creating hierarchy level '{HierarchyLevelName}'.", hierarchyLevelName);
            throw;
        }
    }

    public async Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id)
    {
        try
        {
            return await _dimHierarchyLevelRepository.GetHierarchyLevelByIdAsync(id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "HierarchyLevel {Id} not found", id);
            throw;
        }
    }

    public async Task<IEnumerable<DimHierarchyLevel>> GetAllHierarchyLevelsAsync()
    {
        _logger.LogInformation("Service: Fetching all hierarchy levels.");
        var hierarchyLevels = await _dimHierarchyLevelRepository.GetAllHierarchyLevelsAsync();
        _logger.LogInformation("Service: Fetched {Count} hierarchy levels", hierarchyLevels.Count());
        return hierarchyLevels;
    }

    public async Task<DimHierarchyLevel> UpdateHierarchyLevelAsync(int hierarchyLevelId, string hierarchyLevelCode, string hierarchyLevelName)
    {
        DimHierarchyLevelValidator.ValidateForUpdate(hierarchyLevelId, hierarchyLevelCode, hierarchyLevelName);
        _logger.LogInformation("Service: Attempting to update HierarchyLevel {HierarchyLevelId}", hierarchyLevelId);
        try
        {
            var existingHierarchyLevel = await _dimHierarchyLevelRepository.GetHierarchyLevelByIdAsync(hierarchyLevelId);
                
            existingHierarchyLevel.HierarchyLevelName = hierarchyLevelName;
            existingHierarchyLevel.HierarchyLevelCode = hierarchyLevelCode;

            await _dimHierarchyLevelRepository.UpdateHierarchyLevelAsync(existingHierarchyLevel);
            _logger.LogInformation("Service: Updated HierarchyLevel {HierarchyLevelId}", hierarchyLevelId);
            return existingHierarchyLevel;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Cannot update hierarchy level {HierarchyLevelId}, it was not found.", hierarchyLevelId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict when updating hierarchy level {HierarchyLevelId}.", hierarchyLevelId);
            throw;
        }
    }

    public async Task DeleteHierarchyLevelAsync(int id)
    {
        try
        {
            await _dimHierarchyLevelRepository.DeleteHierarchyLevelAsync(id);
            _logger.LogInformation("Deleted HierarchyLevel {Id}", id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete: hierarchy level {Id} not found.", id);
            throw;
        }
    }
}