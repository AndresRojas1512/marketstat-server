using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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
        DimEducationLevelValidator.ValidateForCreate(educationLevelName);
        var level = new DimEducationLevel(0, educationLevelName);

        try
        {
            await _dimEducationLevelRepository.AddEducationLevelAsync(level);
            _logger.LogInformation("Created education level {EducationLevelId}", level.EducationLevelId);
            return level;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating education level {EducationLevelName} with ID {EducationLevelId}", level.EducationLevelName, level.EducationLevelId);
            throw;
        }
    }

    public async Task<DimEducationLevel> GetEducationLevelByIdAsync(int id)
    {
        try
        {
            return await _dimEducationLevelRepository.GetEducationLevelByIdAsync(id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Education level {Id} not found", id);
            throw;
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
        DimEducationLevelValidator.ValidateForUpdate(id, educationLevelName);
        try
        {
            var existing = await _dimEducationLevelRepository.GetEducationLevelByIdAsync(id);

            existing.EducationLevelName = educationLevelName;

            await _dimEducationLevelRepository.UpdateEducationLevelAsync(existing);
            _logger.LogInformation("Updated {EducationLevelId}", id);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: education level {EducationLevelId} not found", id);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict updating education level {Id}, already exists", id);
            throw;
        }
    }

    public async Task DeleteEducationLevelAsync(int id)
    {
        try
        {
            await _dimEducationLevelRepository.DeleteEducationLevelAsync(id);
            _logger.LogInformation("Deleted education level {EducationLevelId}", id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete: education level {EducationLevelId} not found.", id);
            throw;
        }
    }
}