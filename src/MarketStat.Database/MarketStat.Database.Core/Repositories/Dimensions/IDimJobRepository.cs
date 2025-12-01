namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimJobRepository
{
    Task AddJobAsync(DimJob job);

    Task<DimJob> GetJobByIdAsync(int jobId);

    Task<IEnumerable<DimJob>> GetAllJobsAsync();

    Task UpdateJobAsync(DimJob job);

    Task DeleteJobAsync(int jobId);

    Task<List<int>> GetJobIdsByFilterAsync(
        string? standardJobRoleTitle,
        string? hierarchyLevelName,
        int? industryFieldId);

    Task<IEnumerable<string>> GetDistinctStandardJobRolesAsync(int? industryFieldId);

    Task<IEnumerable<string>> GetDistinctHierarchyLevelsAsync(int? industryFieldId, string? standardJobRoleTitle);
}
