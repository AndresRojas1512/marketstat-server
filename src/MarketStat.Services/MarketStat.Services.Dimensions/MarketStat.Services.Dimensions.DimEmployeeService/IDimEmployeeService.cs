using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployeeService;

public interface IDimEmployeeService
{
    Task<DimEmployee> CreateEmployeeAsync(string employeeRefId, DateOnly birthDate, DateOnly careerStartDate, string? gender, int? educationId, short? graduationYear);
    Task<DimEmployee> GetEmployeeByIdAsync(int employeeId);
    Task<IEnumerable<DimEmployee>> GetAllEmployeesAsync();
    Task<DimEmployee> UpdateEmployeeAsync(int employeeId, string employeeRefId, DateOnly birthDate, DateOnly careerStartDate, string? gender, int? educationId, short? graduationYear);
    Task DeleteEmployeeAsync(int employeeId);
    Task<DimEmployee> PartialUpdateEmployeeAsync(int employeeId, string? employeeRefId, DateOnly? careerStartDate,
        int? educationId, short? graduationYear);
}