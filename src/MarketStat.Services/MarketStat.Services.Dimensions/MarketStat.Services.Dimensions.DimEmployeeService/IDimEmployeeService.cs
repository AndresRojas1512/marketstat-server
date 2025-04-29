using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployeeService;

public interface IDimEmployeeService
{
    Task<DimEmployee> CreateEmployeeAsync(DateOnly birthDate, DateOnly careerStartDate);
    Task<DimEmployee> GetEmployeeByIdAsync(int employeeId);
    Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync();
    Task<DimEmployee> UpdateEmployeeAsync(int employeeId, DateOnly birthDate, DateOnly careerStartDate);
    Task DeleteEmployeeAsync(int employeeId);
}