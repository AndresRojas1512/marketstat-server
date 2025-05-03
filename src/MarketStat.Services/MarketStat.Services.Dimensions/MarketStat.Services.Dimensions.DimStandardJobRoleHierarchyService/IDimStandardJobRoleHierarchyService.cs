using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService;

public interface IDimStandardJobRoleHierarchyService
{
    Task<DimStandardJobRoleHierarchy> CreateStandardJobRoleHierarchy(int jobRoleId, int levelId);
    Task<DimStandardJobRoleHierarchy> GetStandardJobRoleHierarchyAsync(int jobRoleId, int levelId);
    Task<IEnumerable<DimStandardJobRoleHierarchy>> GetLevelsByJobRoleIdAsync(int jobRoleId);
    Task<IEnumerable<DimStandardJobRoleHierarchy>> GetJobRolesByLevelIdAsync(int levelId);
    Task<IEnumerable<DimStandardJobRoleHierarchy>> GetAllStandardJobRoleHierarchiesAsync();
    Task DeleteStandardJobRoleHierarchyAsync(int jobRoleId, int levelId);
}