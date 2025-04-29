using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployeeEducationService;

public interface IDimEmployeeEducationService
{
    Task<DimEmployeeEducation> CreateEmployeeEducationAsync(int employeeId, int educationId, short graduationYear);
    Task<DimEmployeeEducation> GetEmployeeEducationAsync(int employeeId, int educationId);
    Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<DimEmployeeEducation>> GetAllEmployeeEducationsAsync();
    Task<DimEmployeeEducation> UpdateEmployeeEducationAsync(int employeeId, int educationId, short graduationYear);
    Task DeleteEmployeeEducationAsync(int employeeId, int educationId);
    
}