using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEmployeeRepository
{
    Task AddEmployeeAsync(DimEmployee employee);
    Task<DimEmployee> GetEmployeeByIdAsync(int employeeId);
    Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync();
    Task UpdateEmployeeAsync(DimEmployee employee);
    Task DeleteEmployeeAsync(int employeeId);
}