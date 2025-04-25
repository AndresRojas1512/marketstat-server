using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimJobRoleRepository
{
    Task AddJobRoleAsync(DimJobRole jobRole);
    Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId);
    Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync();
    Task UpdateJobRoleAsync(DimJobRole jobRole);
    Task DeleteJobRoleAsync(int jobRoleId);
}