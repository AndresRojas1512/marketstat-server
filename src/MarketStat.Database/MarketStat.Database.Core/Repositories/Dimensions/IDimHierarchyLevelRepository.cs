using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimHierarchyLevelRepository
{
    Task AddHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel);
    Task<IEnumerable<DimHierarchyLevel>> GetAllHierarchyLevelsAsync();
    Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id);
    Task UpdateHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel);
    Task DeleteHierarchyLevelAsync(int id);
}