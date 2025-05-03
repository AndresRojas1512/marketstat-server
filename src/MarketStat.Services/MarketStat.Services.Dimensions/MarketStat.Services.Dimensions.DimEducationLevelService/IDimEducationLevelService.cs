using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEducationLevelService;

public interface IDimEducationLevelService
{
    Task<DimEducationLevel> CreateEducationLevelAsync(string educationLevelName);
    Task<DimEducationLevel> GetEducationLevelByIdAsync(int id);
    Task<IEnumerable<DimEducationLevel>> GetAllEducationLevelsAsync();
    Task<DimEducationLevel> UpdateEducationLevelAsync(int id, string educationLevelName);
    Task DeleteEducationLevelAsync(int id);
}