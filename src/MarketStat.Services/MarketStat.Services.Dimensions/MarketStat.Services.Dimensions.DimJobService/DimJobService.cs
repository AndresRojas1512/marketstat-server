namespace MarketStat.Services.Dimensions.DimJobService;

using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimJobService.Validators;
using Microsoft.Extensions.Logging;

public class DimJobService : IDimJobService
{
    private readonly IDimJobRepository _dimJobRepository;
    private readonly ILogger<DimJobService> _logger;

    public DimJobService(IDimJobRepository dimJobRepository, ILogger<DimJobService> logger)
    {
        _dimJobRepository = dimJobRepository;
        _logger = logger;
    }

    public async Task<DimJob> CreateJobAsync(
        string jobRoleTitle,
        string standardJobRoleTitle,
        string hierarchyLevelName,
        int industryFieldId)
    {
        DimJobValidator.ValidateForCreate(jobRoleTitle, standardJobRoleTitle, hierarchyLevelName, industryFieldId);
        var job = new DimJob(0, jobRoleTitle, standardJobRoleTitle, hierarchyLevelName, industryFieldId);
        await _dimJobRepository.AddJobAsync(job).ConfigureAwait(false);
        _logger.LogInformation("Created job {JobId}", job.JobId);
        return job;
    }

    public async Task<DimJob> GetJobByIdAsync(int jobId)
    {
        return await _dimJobRepository.GetJobByIdAsync(jobId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<DimJob>> GetAllJobsAsync()
    {
        var list = await _dimJobRepository.GetAllJobsAsync().ConfigureAwait(false);
        _logger.LogInformation("Fetched {Count} job records", list.Count());
        return list;
    }

    public async Task<DimJob> UpdateJobAsync(
        int jobId,
        string jobRoleTitle,
        string standardJobRoleTitle,
        string hierarchyLevelName,
        int industryFieldId)
    {
        DimJobValidator.ValidateForUpdate(
            jobId,
            jobRoleTitle,
            standardJobRoleTitle,
            hierarchyLevelName,
            industryFieldId);

        var existing = await _dimJobRepository.GetJobByIdAsync(jobId).ConfigureAwait(false);
        existing.JobRoleTitle = jobRoleTitle;
        existing.StandardJobRoleTitle = standardJobRoleTitle;
        existing.HierarchyLevelName = hierarchyLevelName;
        existing.IndustryFieldId = industryFieldId;

        await _dimJobRepository.UpdateJobAsync(existing).ConfigureAwait(false);
        _logger.LogInformation("Updated job {JobId}", jobId);
        return existing;
    }

    public async Task DeleteJobAsync(int jobId)
    {
        await _dimJobRepository.DeleteJobAsync(jobId).ConfigureAwait(false);
        _logger.LogInformation("Deleted job {JobId}", jobId);
    }

    public async Task<IEnumerable<string>> GetDistinctStandardJobRolesAsync(int? industryFieldId)
    {
        _logger.LogInformation(
            "Fetching distinct standard job roles for IndustryFieldId: {IndustryFieldId}",
            industryFieldId);
        return await _dimJobRepository.GetDistinctStandardJobRolesAsync(industryFieldId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetDistinctHierarchyLevelsAsync(
        int? industryFieldId,
        string? standardJobRoleTitle)
    {
        _logger.LogInformation(
            "Fetching distinct hierarchy levels for IndustryFieldId: {IndustryFieldId} and StandardJobRoleTitle: {StandardJobRoleTitle}", industryFieldId, standardJobRoleTitle);
        return await _dimJobRepository.GetDistinctHierarchyLevelsAsync(industryFieldId, standardJobRoleTitle).ConfigureAwait(false);
    }
}
