namespace MarketStat.Services.Dimensions.DimJobService;

using MarketStat.Common.Core.Dimensions;

public interface IDimJobService
{
    Task<DimJob> CreateJobAsync(
        string jobRoleTitle,
        string standardJobRoleTitle,
        string hierarchyLevelName,
        int industryFieldId);

    Task<DimJob> GetJobByIdAsync(int jobId);

    Task<IEnumerable<DimJob>> GetAllJobsAsync();

    Task<DimJob> UpdateJobAsync(
        int jobId,
        string jobRoleTitle,
        string standardJobRoleTitle,
        string hierarchyLevelName,
        int industryFieldId);

    Task DeleteJobAsync(int jobId);

    Task<IEnumerable<string>> GetDistinctStandardJobRolesAsync(int? industryFieldId);

    Task<IEnumerable<string>> GetDistinctHierarchyLevelsAsync(int? industryFieldId, string? standardJobRoleTitle);
}
