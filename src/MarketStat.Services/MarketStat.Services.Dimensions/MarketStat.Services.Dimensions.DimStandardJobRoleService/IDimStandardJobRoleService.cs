using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimStandardJobRoleService;

public interface IDimStandardJobRoleService
{
    Task<DimStandardJobRole> CreateStandardJobRoleAsync(string jobRoleTitle, int industryFieldId);
    Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id);
    Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync();
    Task<DimStandardJobRole> UpdateStandardJobRoleAsync(int id, string jobRoleTitle, int industryFieldId);
    Task DeleteStandardJobRoleAsync(int id);
}