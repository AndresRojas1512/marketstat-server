using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeEducationService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEmployeeEducationService;

public class DimEmployeeEducationService
{
    private readonly IDimEmployeeEducationRepository _DimEmployeeEducationRepository;
    private readonly ILogger<DimEmployeeEducationService> _logger;

    public DimEmployeeEducationService(IDimEmployeeEducationRepository dimEmployeeEducationRepository,
        ILogger<DimEmployeeEducationService> logger)
    {
        _DimEmployeeEducationRepository = dimEmployeeEducationRepository;
        _logger = logger;
    }
    
    public async Task AddEmployeeEducationAsync(int employeeId, int educationId)
    {
        DimEmployeeEducationValidator.ValidateParameters(employeeId, educationId);

        var link = new DimEmployeeEducation(employeeId, educationId);
        try
        {
            await _DimEmployeeEducationRepository.AddEmployeeEducationAsync(link);
            _logger.LogInformation("Added link Employee {EmployeeId} → Education {EducationId}", employeeId, educationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add EmployeeEducation link");
            throw new Exception($"Could not add education {educationId} to employee {employeeId}");
        }
    }
    
    public async Task RemoveEmployeeEducationAsync(int employeeId, int educationId)
    {
        DimEmployeeEducationValidator.ValidateParameters(employeeId, educationId);

        try
        {
            await _DimEmployeeEducationRepository.RemoveEmployeeEducationAsync(employeeId, educationId);
            _logger.LogInformation("Removed link Employee {EmployeeId} → Education {EducationId}", employeeId, educationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove EmployeeEducation link");
            throw new Exception($"Could not remove education {educationId} from employee {employeeId}");
        }
    }
    
    public Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId)
    {
        if (employeeId <= 0)
            throw new ArgumentException("EmployeeId must be a positive integer.");

        return _DimEmployeeEducationRepository.GetEducationsByEmployeeIdAsync(employeeId);
    }
}