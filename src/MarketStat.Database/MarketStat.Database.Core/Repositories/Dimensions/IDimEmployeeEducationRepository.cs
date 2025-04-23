using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEmployeeEducationRepository
{
    Task AddEmployeeEducationAsync(DimEmployeeEducation link);
    Task RemoveEmployeeEducationAsync(int employeeId, int educationId);
    Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId);
}