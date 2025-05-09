using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployeeEducationService.Validators;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace MarketStat.Services.Dimensions.DimEmployeeEducationService;

public class DimEmployeeEducationService : IDimEmployeeEducationService
{
    private readonly IDimEmployeeEducationRepository _dimEmployeeEducationRepository;
    private readonly ILogger<DimEmployeeEducationService> _logger;

    public DimEmployeeEducationService(IDimEmployeeEducationRepository dimEmployeeEducationRepository,
        ILogger<DimEmployeeEducationService> logger)
    {
        _dimEmployeeEducationRepository = dimEmployeeEducationRepository;
        _logger = logger;
    }

    public async Task<DimEmployeeEducation> CreateEmployeeEducationAsync(int employeeId, int educationId, short graduationYear)
    {
        DimEmployeeEducationValidator.ValidateParameters(employeeId, educationId, graduationYear);
        var link = new DimEmployeeEducation(employeeId, educationId, graduationYear);
        try
        {
            await _dimEmployeeEducationRepository.AddEmployeeEducationAsync(link);
            _logger.LogInformation("Linked employee {EmployeeId} & education {EducationId}", employeeId, educationId);
            return link;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict linking employee {EmployeeId} & education {EducationId}", employeeId,
                educationId);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "FK not found when linking Employee {EmployeeId} & Education {EducationId}", employeeId, educationId);
            throw;
        }
    }

    public async Task<DimEmployeeEducation> GetEmployeeEducationAsync(int employeeId, int educationId)
    {
        try
        {
            return await _dimEmployeeEducationRepository.GetEmployeeEducationAsync(employeeId, educationId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "EmployeeEducation link ({EmployeeId}, {EducationId}) not found.", employeeId, educationId);
            throw;
        }
    }

    public async Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId)
    {
        try
        {
            var list = await _dimEmployeeEducationRepository.GetEducationsByEmployeeIdAsync(employeeId);
            _logger.LogInformation("Fetched {Count} total Education for Employee {EmployeeId}.", list.Count(), employeeId);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No Education found for {EmployeeId},", employeeId);
            throw new Exception($"No Education found for Employee {employeeId}.");
        }
    }

    public async Task<IEnumerable<DimEmployeeEducation>> GetAllEmployeeEducationsAsync()
    {
        var list = (await _dimEmployeeEducationRepository.GetAllEmployeeEducationsAsync()).ToList();
        _logger.LogInformation("Fetched {Count} total EmployeeEducation links.", list.Count);
        return list;
    }

    public async Task<DimEmployeeEducation> UpdateEmployeeEducationAsync(int employeeId, int educationId,
        short graduationYear)
    {
        DimEmployeeEducationValidator.ValidateParameters(employeeId, educationId, graduationYear);
        try
        {
            var existing = await _dimEmployeeEducationRepository.GetEmployeeEducationAsync(employeeId, educationId);
            existing.GraduationYear = graduationYear;
            await _dimEmployeeEducationRepository.UpdateEmployeeEducationAsync(existing);
            _logger.LogInformation("Updated DimEmployeeEducation ({EmployeeId}, {EducationId}).", employeeId,
                educationId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: EmployeeEducation link ({EmployeeId}, {EducationId}) not found.", employeeId, educationId);
            throw;
        }
    }

    public async Task DeleteEmployeeEducationAsync(int employeeId, int educationId)
    {
        try
        {
            await _dimEmployeeEducationRepository.DeleteEmployeeEducationAsync(employeeId, educationId);
            _logger.LogInformation("Removed link Employee {EmployeeId} → Education {EducationId}", employeeId, educationId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete EmployeeEducation link ({EmployeeId},{EducationId})", employeeId, educationId);
            throw;
        }
    }
}