using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimJobService;

public interface IDimJobService
{
    Task<DimJob> CreateJobAsync(string jobRoleTitle, string standardJobRoleTitle, string hierarchyLevelName,
        int industryFieldId);
    Task<DimJob> GetJobByIdAsync(int jobId);
    Task<IEnumerable<DimJob>> GetAllJobsAsync();
    Task<DimJob> UpdateJobAsync(int jobId, string jobRoleTitle, string standardJobRoleTitle, string hierarchyLevelName,
        int industryFieldId);
    Task DeleteJobAsync(int jobId);
}