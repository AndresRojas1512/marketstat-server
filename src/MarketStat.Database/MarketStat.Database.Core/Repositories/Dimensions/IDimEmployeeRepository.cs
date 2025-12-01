namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimEmployeeRepository
{
    Task AddEmployeeAsync(DimEmployee employee);

    Task<DimEmployee> GetEmployeeByIdAsync(int employeeId);

    Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync();

    Task UpdateEmployeeAsync(DimEmployee employee);

    Task DeleteEmployeeAsync(int employeeId);
}
