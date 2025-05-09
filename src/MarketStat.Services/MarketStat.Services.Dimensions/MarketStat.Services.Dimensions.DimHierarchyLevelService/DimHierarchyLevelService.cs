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

    public async Task<DimHierarchyLevel> CreateHierarchyLevelAsync(string hierarchyLevelName)
    {
        DimHierarchyLevelValidator.ValidateForCreate(hierarchyLevelName);
        var hierarchyLevel = new DimHierarchyLevel(0, hierarchyLevelName);
        try
        {
            await _dimHierarchyLevelRepository.AddHierarchyLevelAsync(hierarchyLevel);
            _logger.LogInformation("Created DimHierarchyLevel {HierarchyLevelId}", hierarchyLevel.HierarchyLevelId);
            return hierarchyLevel;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "A hierarchy level {HierarchyLevelId} with name '{HierarchyLevelName}' already exists", hierarchyLevel.HierarchyLevelId, hierarchyLevel.HierarchyLevelName);
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
        var hierarchyLevels = await _dimHierarchyLevelRepository.GetAllHierarchyLevelsAsync();
        _logger.LogInformation("Fetched {Count} hierarchy levels", hierarchyLevels.Count());
        return hierarchyLevels;
    }

    public async Task<DimHierarchyLevel> UpdateHierarchyLevelAsync(int hierarchyLevelId, string hierarchyLevelName)
    {
        DimHierarchyLevelValidator.ValidateForUpdate(hierarchyLevelId, hierarchyLevelName);
        try
        {
            var existingHierarchyLevel =
                await _dimHierarchyLevelRepository.GetHierarchyLevelByIdAsync(hierarchyLevelId);
            existingHierarchyLevel.HierarchyLevelName = hierarchyLevelName;
            await _dimHierarchyLevelRepository.UpdateHierarchyLevelAsync(existingHierarchyLevel);
            _logger.LogInformation("Updated HierarchyLevel {HierarchyLevelId}", hierarchyLevelId);
            return existingHierarchyLevel;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update hierarchy level {HierarchyLevelId}", hierarchyLevelId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict updating hierarchy level {HierarchyLevelId}, already exists", hierarchyLevelId);
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