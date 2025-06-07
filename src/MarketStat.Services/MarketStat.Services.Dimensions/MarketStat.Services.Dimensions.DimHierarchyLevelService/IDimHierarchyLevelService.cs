using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimHierarchyLevelService;

public interface IDimHierarchyLevelService
{
    Task<DimHierarchyLevel> CreateHierarchyLevelAsync(string hierarchyLevelCode, string hierarchyLevelName);
    Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id);
    Task<IEnumerable<DimHierarchyLevel>> GetAllHierarchyLevelsAsync();
    Task<DimHierarchyLevel> UpdateHierarchyLevelAsync(int hierarchyLevelId, string hierarchyLevelCode, string hierarchyLevelName);
    Task DeleteHierarchyLevelAsync(int id);
}