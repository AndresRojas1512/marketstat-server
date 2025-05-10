using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimJobRoleService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimJobRoleService;

public class DimJobRoleService : IDimJobRoleService
{
    private readonly IDimJobRoleRepository _dimJobRoleRepository;
    private readonly ILogger<DimJobRoleService> _logger;

    public DimJobRoleService(IDimJobRoleRepository dimJobRoleRepository, ILogger<DimJobRoleService> logger)
    {
        _dimJobRoleRepository = dimJobRoleRepository;
        _logger = logger;
    }
    
    public async Task<DimJobRole> CreateJobRoleAsync(string jobRoleTitle, int standardJobRoleId, int hierarchyLevelId)
    {
        DimJobRoleValidator.ValidateForCreate(jobRoleTitle, standardJobRoleId, hierarchyLevelId);
        var role = new DimJobRole(0, jobRoleTitle, standardJobRoleId, hierarchyLevelId);
        try
        {
            await _dimJobRoleRepository.AddJobRoleAsync(role);
            _logger.LogInformation("Created job role {JobRoleId}", role.JobRoleId);
            return role;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex,
                "Failed to create DimJobRole (duplicate {JobRoleId})", role.JobRoleId);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Either standard job role or hierarchy level FKs not found");
            throw;
        }
    }
    
    public async Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId)
    {
        try
        {
            return await _dimJobRoleRepository.GetJobRoleByIdAsync(jobRoleId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "JobRole {JobRoleId} not found", jobRoleId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync()
    {
        var list = await _dimJobRoleRepository.GetAllJobRolesAsync();
        _logger.LogInformation("Fetched {Count} JobRole records", list.Count());
        return list;
    }
    
    public async Task<DimJobRole> UpdateJobRoleAsync(int jobRoleId, string jobRoleTitle, int standardJobRoleId, int hierarchyLevelId)
    {
        DimJobRoleValidator.ValidateForUpdate(jobRoleId, jobRoleTitle, standardJobRoleId, hierarchyLevelId);
        try
        {
            var existing = await _dimJobRoleRepository.GetJobRoleByIdAsync(jobRoleId);
            
            existing.JobRoleTitle   = jobRoleTitle;
            existing.StandardJobRoleId = standardJobRoleId;
            existing.HierarchyLevelId = hierarchyLevelId;

            await _dimJobRoleRepository.UpdateJobRoleAsync(existing);
            _logger.LogInformation("Updated DimJobRole {JobRoleId}", jobRoleId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: job role {JobRoleId} not found", jobRoleId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex,
                "Conflict updating job role {JobRoleId}", jobRoleId);
            throw;
        }
    }
    
    public async Task DeleteJobRoleAsync(int jobRoleId)
    {
        try
        {
            await _dimJobRoleRepository.DeleteJobRoleAsync(jobRoleId);
            _logger.LogInformation("Deleted DimJobRole {JobRoleId}", jobRoleId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete: DimJobRole {JobRoleId} not found", jobRoleId);
            throw;
        }
    }
}