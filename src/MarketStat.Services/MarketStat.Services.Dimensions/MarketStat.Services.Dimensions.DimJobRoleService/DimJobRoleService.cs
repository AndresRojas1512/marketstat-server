using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
        var all = (await _dimJobRoleRepository.GetAllJobRolesAsync()).ToList();
        var newId = all.Any() ? all.Max(r => r.JobRoleId) + 1 : 1;
        DimJobRoleValidator.ValidateParameters(newId, jobRoleTitle, standardJobRoleId, hierarchyLevelId);
        var role = new DimJobRole(newId, jobRoleTitle, standardJobRoleId, hierarchyLevelId);
        try
        {
            await _dimJobRoleRepository.AddJobRoleAsync(role);
            _logger.LogInformation("Created DimJobRole {JobRoleId}", newId);
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimJobRole (duplicate {JobRoleId})", newId);
            throw new Exception($"Could not create job role '{jobRoleTitle}'");
        }
    }
    
    public async Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId)
    {
        try
        {
            return await _dimJobRoleRepository.GetJobRoleByIdAsync(jobRoleId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JobRole {JobRoleId} not found", jobRoleId);
            throw new Exception($"Job role {jobRoleId} was not found.");
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
        DimJobRoleValidator.ValidateParameters(jobRoleId, jobRoleTitle, standardJobRoleId, hierarchyLevelId);
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
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update, JobRole {JobRoleId} not found", jobRoleId);
            throw new Exception($"Cannot update: job role {jobRoleId} not found.");
        }
    }
    
    public async Task DeleteJobRoleAsync(int jobRoleId)
    {
        try
        {
            await _dimJobRoleRepository.DeleteJobRoleAsync(jobRoleId);
            _logger.LogInformation("Deleted DimJobRole {JobRoleId}", jobRoleId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete, DimJobRole {JobRoleId} not found", jobRoleId);
            throw new Exception($"Cannot delete: job role {jobRoleId} not found.");
        }
    }
}