using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimJobRoleService;

public interface IDimJobRoleService
{
    Task<DimJobRole> CreateJobRoleAsync(string jobRoleTitle, string seniorityLevel, int industryFieldId);
    Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId);
    Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync();
    Task<DimJobRole> UpdateJobRoleAsync(int jobRoleId, string jobRoleTitle, string seniorityLevel, int industryFieldId);
    Task DeleteJobRoleAsync(int jobRoleId);
}