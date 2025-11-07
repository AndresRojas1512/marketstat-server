using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimJobService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimJobService;

public class DimJobService : IDimJobService
{
    private readonly IDimJobRepository _dimJobRepository;
    private readonly ILogger<DimJobService> _logger;

    public DimJobService(IDimJobRepository dimJobRepository, ILogger<DimJobService> logger)
    {
        _dimJobRepository = dimJobRepository;
        _logger = logger;
    }
    
    public async Task<DimJob> CreateJobAsync(string jobRoleTitle, string standardJobRoleTitle,
        string hierarchyLevelName, int industryFieldId)
    {
        DimJobValidator.ValidateForCreate(jobRoleTitle, standardJobRoleTitle, hierarchyLevelName, industryFieldId);
        var job = new DimJob(0, jobRoleTitle, standardJobRoleTitle, hierarchyLevelName, industryFieldId);
        await _dimJobRepository.AddJobAsync(job);
        _logger.LogInformation("Created job {JobId}", job.JobId);
        return job;
    }

    public async Task<DimJob> GetJobByIdAsync(int jobId)
    {
        return await _dimJobRepository.GetJobByIdAsync(jobId);
    }

    public async Task<IEnumerable<DimJob>> GetAllJobsAsync()
    {
        var list = await _dimJobRepository.GetAllJobsAsync();
        _logger.LogInformation("Fetched {Count} job records", list.Count());
        return list;
    }

    public async Task<DimJob> UpdateJobAsync(int jobId, string jobRoleTitle, string standardJobRoleTitle,
        string hierarchyLevelName, int industryFieldId)
    {
        DimJobValidator.ValidateForUpdate(jobId, jobRoleTitle, standardJobRoleTitle, hierarchyLevelName,
            industryFieldId);

        var existing = await _dimJobRepository.GetJobByIdAsync(jobId);
        existing.JobRoleTitle = jobRoleTitle;
        existing.StandardJobRoleTitle = standardJobRoleTitle;
        existing.HierarchyLevelName = hierarchyLevelName;
        existing.IndustryFieldId = industryFieldId;
        
        await _dimJobRepository.UpdateJobAsync(existing);
        _logger.LogInformation("Updated job {JobId}", jobId);
        return existing;
    }

    public async Task DeleteJobAsync(int jobId)
    {
        await _dimJobRepository.DeleteJobAsync(jobId);
        _logger.LogInformation("Deleted job {JobId}", jobId);
    }

    public async Task<IEnumerable<string>> GetDistinctStandardJobRolesAsync(int? industryFieldId)
    {
        _logger.LogInformation("Fetching distinct standard job roles for IndustryFieldId: {IndustryFieldId}",
            industryFieldId);
        return await _dimJobRepository.GetDistinctStandardJobRolesAsync(industryFieldId);
    }

    public async Task<IEnumerable<string>> GetDistinctHierarchyLevelsAsync(int? industryFieldId,
        string? standardJobRoleTitle)
    {
        _logger.LogInformation(
            "Fetching distinct hierarchy levels for IndustryFieldId: {IndustryFieldId} and StandardJobRoleTitle: {StandardJobRoleTitle}",
            industryFieldId, standardJobRoleTitle);
        return await _dimJobRepository.GetDistinctHierarchyLevelsAsync(industryFieldId, standardJobRoleTitle);
    }
}