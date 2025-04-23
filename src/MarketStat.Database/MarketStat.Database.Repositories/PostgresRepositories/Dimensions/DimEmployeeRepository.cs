using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployeeRepository : IDimEmployeeRepository
{
    private readonly Dictionary<int, DimEmployee> _employees = new Dictionary<int, DimEmployee>();
    
    public Task AddEmployeeAsync(DimEmployee employee)
    {
        if (!_employees.TryAdd(employee.EmployeeId, employee))
        {
            throw new ArgumentException($"Employee {employee.EmployeeId} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task<DimEmployee> GetEmployeeByIdAsync(int employeeId)
    {
        if (_employees.TryGetValue(employeeId, out var e))
        {
            return Task.FromResult(e);
        }
        throw new KeyNotFoundException($"Employee {employeeId} not found.");
    }

    public Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync()
    {
        return Task.FromResult<IEnumerable<DimEmployee>>(_employees.Values);
    }

    public Task UpdateEmployeeAsync(DimEmployee employee)
    {
        if (!_employees.ContainsKey(employee.EmployeeId))
        {
            throw new KeyNotFoundException($"Cannot update: employee {employee.EmployeeId} not found.");
        }
        _employees[employee.EmployeeId] = employee;
        return Task.CompletedTask;
    }

    public Task DeleteEmployeeAsync(int employeeId)
    {
        if (!_employees.ContainsKey(employeeId))
        {
            throw new KeyNotFoundException($"Cannot delete: employee {employeeId} not found.");
        }
        return Task.CompletedTask;
    }
}