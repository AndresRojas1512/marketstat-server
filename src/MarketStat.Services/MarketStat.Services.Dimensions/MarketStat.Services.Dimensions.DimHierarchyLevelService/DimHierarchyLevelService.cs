using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
        var allHierarchyLevels = (await _dimHierarchyLevelRepository.GetAllHierarchyLevelsAsync()).ToList();
        var newId = allHierarchyLevels.Any() ? allHierarchyLevels.Max(h => h.HierarchyLevelId) + 1 : 1;
        DimHierarchyLevelValidator.ValidateParameters(newId, hierarchyLevelName);
        var hierarchyLevel = new DimHierarchyLevel(newId, hierarchyLevelName);
        try
        {
            await _dimHierarchyLevelRepository.AddHierarchyLevelAsync(hierarchyLevel);
            _logger.LogInformation("Created DimHierarchyLevel {HierarchyLevelId}", newId);
            return hierarchyLevel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create HierarchyLevel {HierarchyLevelId}", newId);
            throw new Exception($"Could not create HierarchyLevel {hierarchyLevelName} with id {newId},");
        }
        
    }

    public async Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id)
    {
        try
        {
            return await _dimHierarchyLevelRepository.GetHierarchyLevelByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HierarchyLevel {Id} not found", id);
            throw new Exception($"Industry field {id} was not found.");
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
        DimHierarchyLevelValidator.ValidateParameters(hierarchyLevelId, hierarchyLevelName);
        try
        {
            var existingHierarchyLevel = await _dimHierarchyLevelRepository.GetHierarchyLevelByIdAsync(hierarchyLevelId);
            existingHierarchyLevel.HierarchyLevelName = hierarchyLevelName;
            await _dimHierarchyLevelRepository.UpdateHierarchyLevelAsync(existingHierarchyLevel);
            _logger.LogInformation("Updated HierarchyLevel {HierarchyLevelId}", hierarchyLevelId);
            return existingHierarchyLevel;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update hierarchy level {HierarchyLevelId}", hierarchyLevelId);
            throw new Exception($"Cannot update: hierarchy level {hierarchyLevelId} not found.");
        }
    }

    public async Task DeleteHierarchyLevelAsync(int id)
    {
        try
        {
            await _dimHierarchyLevelRepository.DeleteHierarchyLevelAsync(id);
            _logger.LogInformation("Deleted HierarchyLevel {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete hierarchy level {Id}.", id);
            throw new Exception($"Cannot delete: hierarchy level {id} not found.");
        }
    }
}