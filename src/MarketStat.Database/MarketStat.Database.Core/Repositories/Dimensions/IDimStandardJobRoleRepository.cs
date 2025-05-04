using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimStandardJobRoleRepository
{
    Task AddStandardJobRoleAsync(DimStandardJobRole jobRole);
    Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id);
    Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync();
    Task UpdateStandardJobRoleAsync(DimStandardJobRole jobRole);
    Task DeleteStandardJobRoleAsync(int id);
}