using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEmployeeService;

public class DimEmployeeService : IDimEmployeeService
{
    private readonly IDimEmployeeRepository _dimEmployeeRepository;
    private readonly ILogger<DimEmployeeService> _logger;

    public DimEmployeeService(IDimEmployeeRepository dimEmployeeRepository, ILogger<DimEmployeeService> logger)
    {
        _dimEmployeeRepository = dimEmployeeRepository;
        _logger = logger;
    }
    
    public async Task<DimEmployee> CreateEmployeeAsync(DateOnly birthDate, DateOnly careerStartDate)
    {
        DimEmployeeValidator.ValidateForCreate(birthDate, careerStartDate);
        var employee = new DimEmployee(0, birthDate, careerStartDate);

        try
        {
            await _dimEmployeeRepository.AddEmployeeAsync(employee);
            _logger.LogInformation("Created DimEmployee {EmployeeId}", employee.EmployeeId);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimEmployee {EmployeeId}", employee.EmployeeId);
            throw new Exception($"Could not create employee {employee.EmployeeId}");
        }
    }
    
    public async Task<DimEmployee> GetEmployeeByIdAsync(int employeeId)
    {
        try
        {
            return await _dimEmployeeRepository.GetEmployeeByIdAsync(employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Employee {EmployeeId} not found", employeeId);
            throw new Exception($"Employee {employeeId} was not found.");
        }
    }
    
    public async Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync()
    {
        var list = await _dimEmployeeRepository.GetAllEmployeesAsync();
        _logger.LogInformation("Fetched {Count} employees", list.Count());
        return list;
    }
    
    public async Task<DimEmployee> UpdateEmployeeAsync(int employeeId, DateOnly birthDate, DateOnly careerStartDate)
    {
        DimEmployeeValidator.ValidateForUpdate(employeeId, birthDate, careerStartDate);
        try
        {
            var existing = await _dimEmployeeRepository.GetEmployeeByIdAsync(employeeId);
            existing.BirthDate = birthDate;
            existing.CareerStartDate = careerStartDate;
            await _dimEmployeeRepository.UpdateEmployeeAsync(existing);
            _logger.LogInformation("Updated DimEmployee {EmployeeId}", employeeId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update employee {EmployeeId}", employeeId);
            throw new Exception($"Cannot update: employee {employeeId} not found.");
        }
    }
    
    public async Task DeleteEmployeeAsync(int employeeId)
    {
        try
        {
            await _dimEmployeeRepository.DeleteEmployeeAsync(employeeId);
            _logger.LogInformation("Deleted DimEmployee {EmployeeId}", employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete employee {EmployeeId}", employeeId);
            throw new Exception($"Cannot delete: employee {employeeId} not found.");
        }
    }
}