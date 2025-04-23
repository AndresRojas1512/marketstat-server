using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimJobRoleRepository : IDimJobRoleRepository
{
    private readonly Dictionary<int, DimJobRole> _jobRoles = new Dictionary<int, DimJobRole>();
    
    public Task AddJobRoleAsync(DimJobRole jobRole)
    {
        if (!_jobRoles.TryAdd(jobRole.JobRoleId, jobRole))
        {
            throw new ArgumentException($"JobRole {jobRole.JobRoleId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId)
    {
        if (_jobRoles.TryGetValue(jobRoleId, out var j))
        {
            return Task.FromResult(j);
        }
        throw new KeyNotFoundException($"JobRole {jobRoleId} not found.");
    }

    public Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync()
    {
        return Task.FromResult<IEnumerable<DimJobRole>>(_jobRoles.Values);
    }

    public Task UpdateJobRoleAsync(DimJobRole jobRole)
    {
        if (!_jobRoles.ContainsKey(jobRole.JobRoleId))
        {
            throw new KeyNotFoundException($"Cannot update: JobRole {jobRole.JobRoleId} not found.");
        }
        _jobRoles[jobRole.JobRoleId] = jobRole;
        return Task.CompletedTask;
    }

    public Task DeleteJobRoleAsync(int jobRoleId)
    {
        if (!_jobRoles.ContainsKey(jobRoleId))
        {
            throw new KeyNotFoundException($"Cannot delete: JobRole {jobRoleId} not found.");
        }
        return Task.CompletedTask;
    }
}