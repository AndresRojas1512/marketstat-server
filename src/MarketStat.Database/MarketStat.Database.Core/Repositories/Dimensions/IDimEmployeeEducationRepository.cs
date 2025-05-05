using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEmployeeEducationRepository
{
    Task AddEmployeeEducationAsync(DimEmployeeEducation link);
    Task<DimEmployeeEducation> GetEmployeeEducationAsync(int employeeId, int educationId);
    Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<DimEmployeeEducation>> GetAllEmployeeEducationsAsync();
    Task UpdateEmployeeEducationAsync(DimEmployeeEducation link);
    Task DeleteEmployeeEducationAsync(int employeeId, int educationId);
}