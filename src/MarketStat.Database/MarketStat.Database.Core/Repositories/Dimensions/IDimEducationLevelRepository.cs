using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEducationLevelRepository
{
    Task AddEducationLevelAsync(DimEducationLevel educationLevel);
    Task<DimEducationLevel> GetEducationLevelByIdAsync(int id);
    Task<IEnumerable<DimEducationLevel>> GetAllEducationLevelsAsync();
    Task UpdateEducationLevelsAsync(DimEducationLevel educationLevel);
    Task DeleteEducationLevelAsync(int id);
}