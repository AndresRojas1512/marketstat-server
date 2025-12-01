using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
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

    public async Task<DimEmployee> CreateEmployeeAsync(string employeeRefId, DateOnly birthDate, DateOnly careerStartDate, string? gender, int? educationId, short? graduationYear)
    {
        DimEmployeeValidator.ValidateForCreate(employeeRefId, birthDate, careerStartDate, gender, educationId, graduationYear);
        _logger.LogInformation("Service: Attempting to create employee with RefId: {EmployeeRefId}", employeeRefId);
        var employee = new DimEmployee(0, employeeRefId, birthDate, careerStartDate, gender, educationId, graduationYear);
        try
        {
            await _dimEmployeeRepository.AddEmployeeAsync(employee).ConfigureAwait(false);
            _logger.LogInformation("Service: Created DimEmployee {EmployeeId} with RefId {EmployeeRefId}", employee.EmployeeId, employee.EmployeeRefId);
            return employee;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict creating employee with RefId '{EmployeeRefId}'.", employee.EmployeeRefId);
            throw;
        }
    }

    public async Task<DimEmployee> GetEmployeeByIdAsync(int employeeId)
    {
        _logger.LogInformation("Service: Fetching employee by ID: {EmployeeId}", employeeId);
        return await _dimEmployeeRepository.GetEmployeeByIdAsync(employeeId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync()
    {
        _logger.LogInformation("Service: Fetching all employees.");
        var list = await _dimEmployeeRepository.GetAllEmployeesAsync().ConfigureAwait(false);
        _logger.LogInformation("Service: Fetched {Count} employee records", list.Count());
        return list;
    }

    public async Task<DimEmployee> UpdateEmployeeAsync(int employeeId, string employeeRefId, DateOnly birthDate, DateOnly careerStartDate, string? gender, int? educationId, short? graduationYear)
    {
        // DimEmployeeValidator.ValidateForUpdate(employeeId, employeeRefId, birthDate, careerStartDate, gender, educationId, graduationYear);
        _logger.LogInformation("Service: Attempting to update DimEmployee {EmployeeId}", employeeId);

        try
        {
            var existingEmployee = await _dimEmployeeRepository.GetEmployeeByIdAsync(employeeId).ConfigureAwait(false);

            existingEmployee.EmployeeRefId = employeeRefId;
            existingEmployee.BirthDate = birthDate;
            existingEmployee.CareerStartDate = careerStartDate;
            existingEmployee.Gender = gender;
            existingEmployee.EducationId = educationId;
            existingEmployee.GraduationYear = graduationYear;

            await _dimEmployeeRepository.UpdateEmployeeAsync(existingEmployee).ConfigureAwait(false);
            _logger.LogInformation("Service: Updated DimEmployee {EmployeeId}", employeeId);
            return existingEmployee;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Cannot update employee {EmployeeId}, as it was not found.", employeeId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict when updating employee {EmployeeId}.", employeeId);
            throw;
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId)
    {
        _logger.LogInformation("Service: Attempting to delete DimEmployee {EmployeeId}", employeeId);
        await _dimEmployeeRepository.DeleteEmployeeAsync(employeeId).ConfigureAwait(false);
        _logger.LogInformation("Service: Deleted DimEmployee {EmployeeId}", employeeId);
    }

    public async Task<DimEmployee> PartialUpdateEmployeeAsync(
        int employeeId,
        string? employeeRefId,
        DateOnly? careerStartDate,
        int? educationId,
        short? graduationYear)
    {
        _logger.LogInformation("Service: Attempting to partially update DimEmployee {EmployeeId}", employeeId);
        try
        {
            var existingEmployee = await _dimEmployeeRepository.GetEmployeeByIdAsync(employeeId).ConfigureAwait(false);

            var newRefId = employeeRefId ?? existingEmployee.EmployeeRefId;
            var newCareerStart = careerStartDate ?? existingEmployee.CareerStartDate;
            var newEducationId = educationId ?? existingEmployee.EducationId;
            var newGradYear = graduationYear ?? existingEmployee.GraduationYear;

            // DimEmployeeValidator.ValidateForUpdate(
            //     employeeId,
            //     newRefId,
            //     existingEmployee.BirthDate,
            //     newCareerStart,
            //     existingEmployee.Gender,
            //     newEducationId,
            //     newGradYear);
            existingEmployee.EmployeeRefId = newRefId;
            existingEmployee.CareerStartDate = newCareerStart;
            existingEmployee.EducationId = newEducationId;
            existingEmployee.GraduationYear = newGradYear;

            await _dimEmployeeRepository.UpdateEmployeeAsync(existingEmployee).ConfigureAwait(false);
            _logger.LogInformation("Service: Partially update DimEmployee {EmployeeId}", employeeId);
            return existingEmployee;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Cannot partially update employee {EmployeeId}. Not found.", employeeId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict when partially updating employee {EmployeeId}.", employeeId);
            throw;
        }
    }
}
