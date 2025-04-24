using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployeeEducationService;

public interface IDimEmployeeEducationService
{
    Task AddEmployeeEducationAsync(int employeeId, int educationId);
    Task RemoveEmployeeEducationAsync(int employeeId, int educationId);
    Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId);
}