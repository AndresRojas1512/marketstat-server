using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;

public class DimStandardJobRoleHierarchyService : IDimStandardJobRoleHierarchyService
{
    private readonly IDimStandardJobRoleHierarchyRepository _dimStandardJobRoleHierarchyRepository;
    private readonly ILogger<DimStandardJobRoleHierarchyService> _logger;

    public DimStandardJobRoleHierarchyService(
        IDimStandardJobRoleHierarchyRepository dimStandardJobRoleHierarchyRepository,
        ILogger<DimStandardJobRoleHierarchyService> logger)
    {
        _dimStandardJobRoleHierarchyRepository = dimStandardJobRoleHierarchyRepository;
        _logger = logger;
    }
    
    public async Task<DimStandardJobRoleHierarchy> CreateStandardJobRoleHierarchy(int jobRoleId, int levelId)
    {
        DimStandardJobRoleHierarchyValidator.ValidateParameters(jobRoleId, levelId);
        var link = new DimStandardJobRoleHierarchy(jobRoleId, levelId);
        try
        {
            await _dimStandardJobRoleHierarchyRepository.AddStandardJobRoleHierarchyAsync(link);
            _logger.LogInformation("Created link ({JobRoleId},{LevelId})", jobRoleId, levelId);
            return link;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating link ({JobRoleId},{LevelId})", jobRoleId, levelId);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Cannot create link: FK missing ({JobRoleId},{LevelId})", jobRoleId, levelId);
            throw;
        }
    }

    public async Task<DimStandardJobRoleHierarchy> GetStandardJobRoleHierarchyAsync(int jobRoleId, int levelId)
    {
        try
        {
            return await _dimStandardJobRoleHierarchyRepository.GetStandardJobRoleHierarchyAsync(jobRoleId, levelId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Link ({JobRoleId},{LevelId}) not found", jobRoleId, levelId);
            throw;
        }
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetLevelsByJobRoleIdAsync(int jobRoleId)
    {
        var list = await _dimStandardJobRoleHierarchyRepository.GetLevelsByJobRoleIdAsync(jobRoleId);
        _logger.LogInformation("Fetched {Count} levels for job {JobRoleId}.", list.Count(), jobRoleId);
        return list;
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetJobRolesByLevelIdAsync(int levelId)
    {
        var list = await _dimStandardJobRoleHierarchyRepository.GetJobRolesByLevelIdAsync(levelId);
        _logger.LogInformation("Fetched {Count} job roles for level {LevelId}.", list.Count(), levelId);
        return list;
    }

    public async Task<IEnumerable<DimStandardJobRoleHierarchy>> GetAllStandardJobRoleHierarchiesAsync()
    {
        var list = await _dimStandardJobRoleHierarchyRepository.GetAllStandardJobRoleHierarchiesAsync();
        _logger.LogInformation("Fetched {Count} total links.", list.Count());
        return list;
    }

    public async Task DeleteStandardJobRoleHierarchyAsync(int jobRoleId, int levelId)
    {
        try
        {
            await _dimStandardJobRoleHierarchyRepository.DeleteStandardJobRoleHierarchyAsync(jobRoleId, levelId);
            _logger.LogInformation("Deleted link ({JobRoleId}, {LevelId}).", jobRoleId, levelId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete link ({JobRoleId},{LevelId})", jobRoleId, levelId);
            throw;
        }
    }
}