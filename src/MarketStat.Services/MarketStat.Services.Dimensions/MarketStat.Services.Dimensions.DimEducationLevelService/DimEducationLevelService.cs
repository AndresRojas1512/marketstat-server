using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationLevelService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEducationLevelService;

public class DimEducationLevelService : IDimEducationLevelService
{
    private readonly IDimEducationLevelRepository _dimEducationLevelRepository;
    private readonly ILogger<DimEducationLevelService> _logger;

    public DimEducationLevelService(IDimEducationLevelRepository dimEducationLevelRepository,
        ILogger<DimEducationLevelService> logger)
    {
        _dimEducationLevelRepository = dimEducationLevelRepository;
        _logger = logger;
    }
    
    public async Task<DimEducationLevel> CreateEducationLevelAsync(string educationLevelName)
    {
        var all = (await _dimEducationLevelRepository.GetAllEducationLevelsAsync()).ToList();
        var newId = all.Any() ? all.Max(x => x.EducationLevelId) + 1 : 1;
        DimEducationLevelValidator.ValidateParameters(newId, educationLevelName);
        var level = new DimEducationLevel(newId, educationLevelName);

        try
        {
            await _dimEducationLevelRepository.AddEducationLevelAsync(level);
            _logger.LogInformation("Created DimEducationLevel {EducationLevelId}", newId);
            return level;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimEducationLevel {EducationLevelId}", newId);
            throw new Exception($"Could not create DimEducationLevel {newId}.");
        }
    }

    public async Task<DimEducationLevel> GetEducationLevelByIdAsync(int id)
    {
        try
        {
            return await _dimEducationLevelRepository.GetEducationLevelByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EducationLevel {Id} not found", id);
            throw new Exception($"EducationLevel {id} was not found.");
        }
    }

    public async Task<IEnumerable<DimEducationLevel>> GetAllEducationLevelsAsync()
    {
        var list = await _dimEducationLevelRepository.GetAllEducationLevelsAsync();
        _logger.LogInformation("Fetched {Count} education levels", list.Count());
        return list;
    }

    public async Task<DimEducationLevel> UpdateEducationLevelAsync(int id, string educationLevelName)
    {
        DimEducationLevelValidator.ValidateParameters(id, educationLevelName);
        try
        {
            var existing = await _dimEducationLevelRepository.GetEducationLevelByIdAsync(id);
            existing.EducationLevelName = educationLevelName;
            await _dimEducationLevelRepository.UpdateEducationLevelsAsync(existing);
            _logger.LogInformation("Updated {EducationLevelId}", id);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot update education level {EducationLevelId}.", id);
            throw new Exception($"Cannot update: education level {id} not found.");
        }
    }

    public async Task DeleteEducationLevelAsync(int id)
    {
        try
        {
            await _dimEducationLevelRepository.DeleteEducationLevelAsync(id);
            _logger.LogInformation("Deleted education level {EducationLevelId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete education level {EducationLevelId}.", id);
            throw new Exception($"Cannot delete: education level {id} not found.");
        }
    }
}