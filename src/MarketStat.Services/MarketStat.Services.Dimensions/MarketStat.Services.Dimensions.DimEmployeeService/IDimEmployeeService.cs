using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployeeService;

public interface IDimEmployeeService
{
    Task<DimEmployee> CreateEmployeeAsync(DateOnly birthDate);
    Task<DimEmployee> GetEmployeeByIdAsync(int employeeId);
    Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync();
    Task<DimEmployee> UpdateEmployeeAsync(int employeeId, DateOnly birthDate);
    Task DeleteEmployeeAsync(int employeeId);
}