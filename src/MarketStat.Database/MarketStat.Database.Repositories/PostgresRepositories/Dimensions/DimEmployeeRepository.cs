using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployeeRepository : BaseRepository, IDimEmployeeRepository
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
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx 
                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An employee with the reference ID '{employee.EmployeeRefId}' already exists.");
        }
        employee.EmployeeId = dbEmployee.EmployeeId;
    }

    public async Task<DimEmployee> GetEmployeeByIdAsync(int employeeId)
    {
        var dbEmployee = await _dbContext.DimEmployees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                
        if (dbEmployee == null)
        {
            throw new NotFoundException($"Employee with ID {employeeId} not found.");
        }
        return DimEmployeeConverter.ToDomain(dbEmployee);
    }

    public async Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync()
    {
        var dbAllEmployees = await _dbContext.DimEmployees
            .AsNoTracking()
            .OrderBy(e => e.EmployeeId)
            .ToListAsync();
        return dbAllEmployees.Select(DimEmployeeConverter.ToDomain);
    }

    public async Task UpdateEmployeeAsync(DimEmployee employee)
    {
        var dbEmployee = await _dbContext.DimEmployees
            .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);

        if (dbEmployee == null)
        {
            throw new NotFoundException($"Employee with ID {employee.EmployeeId} not found.");
        }

        dbEmployee.EmployeeRefId = employee.EmployeeRefId;
        dbEmployee.BirthDate = employee.BirthDate;
        dbEmployee.CareerStartDate = employee.CareerStartDate;
        dbEmployee.Gender = employee.Gender;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx 
                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"Updating employee resulted in a conflict. The reference ID '{employee.EmployeeRefId}' may already be in use by another employee.");
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId)
    {
        var dbEmployee = await _dbContext.DimEmployees.FindAsync(employeeId);
        if (dbEmployee == null)
        {
            throw new NotFoundException($"Employee with ID {employeeId} not found.");
        }
        _dbContext.DimEmployees.Remove(dbEmployee);
        await _dbContext.SaveChangesAsync();
    }
}