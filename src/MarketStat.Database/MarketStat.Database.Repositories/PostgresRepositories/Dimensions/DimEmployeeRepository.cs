using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployeeRepository : IDimEmployeeRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEmployeeRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddEmployeeAsync(DimEmployee employee)
    {
        var dbEmployee = DimEmployeeConverter.ToDbModel(employee);
        await _dbContext.DimEmployees.AddAsync(dbEmployee);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimEmployee> GetEmployeeByIdAsync(int employeeId)
    {
        var dbEmployee = await _dbContext.DimEmployees.FindAsync(employeeId) 
                         ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");
        return DimEmployeeConverter.ToDomain(dbEmployee);
    }

    public async Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync()
    {
        var dbAllEmployees = await _dbContext.DimEmployees.ToListAsync();
        return dbAllEmployees.Select(DimEmployeeConverter.ToDomain);
    }

    public async Task UpdateEmployeeAsync(DimEmployee employee)
    {
        var dbEmployee = await _dbContext.DimEmployees.FindAsync(employee.EmployeeId) 
                         ?? throw new KeyNotFoundException($"Cannot update Employee {employee.EmployeeId}.");
        dbEmployee.EmployeeId = employee.EmployeeId;
        dbEmployee.BirthDate = employee.BirthDate;
        dbEmployee.CareerStartDate = employee.CareerStartDate;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(int employeeId)
    {
        var dbEmployee = await _dbContext.DimEmployees.FindAsync(employeeId) 
                         ?? throw new KeyNotFoundException($"Cannot delete Employee {employeeId}.");
        _dbContext.DimEmployees.Remove(dbEmployee);
        await _dbContext.SaveChangesAsync();
    }
}