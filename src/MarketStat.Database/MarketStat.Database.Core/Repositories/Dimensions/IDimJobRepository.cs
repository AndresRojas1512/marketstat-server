using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimJobRepository
{
    Task AddJobAsync(DimJob job);
    Task<DimJob> GetJobByIdAsync(int jobId);
    Task<IEnumerable<DimJob>> GetAllJobsAsync();
    Task UpdateJobAsync(DimJob job);
    Task DeleteJobAsync(int jobId);
}