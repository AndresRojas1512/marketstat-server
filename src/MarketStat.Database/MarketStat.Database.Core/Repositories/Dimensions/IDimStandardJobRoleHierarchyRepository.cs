using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimStandardJobRoleHierarchyRepository
{
    Task AddStandardJobRoleHierarchyAsync(DimStandardJobRoleHierarchy link);
    Task<DimStandardJobRoleHierarchy> GetStandardJobRoleHierarchyAsync(int jobRoleId, int levelId);
    Task<IEnumerable<DimStandardJobRoleHierarchy>> GetLevelsByJobRoleIdAsync(int jobRoleId);
    Task<IEnumerable<DimStandardJobRoleHierarchy>> GetJobRolesByLevelIdAsync(int levelId);
    Task<IEnumerable<DimStandardJobRoleHierarchy>> GetAllStandardJobRoleHierarchiesAsync();
    Task DeleteStandardJobRoleHierarchyAsync(int jobRoleId, int levelId);
}