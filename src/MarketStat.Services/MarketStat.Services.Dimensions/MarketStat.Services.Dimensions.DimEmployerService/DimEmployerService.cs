using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEmployerService;

public class DimEmployerService : IDimEmployerService
{
    private readonly IDimEmployerRepository _dimEmployerRepository;
    private readonly ILogger<DimEmployerService> _logger;

    public DimEmployerService(IDimEmployerRepository dimEmployerRepository, ILogger<DimEmployerService> logger)
    {
        _dimEmployerRepository = dimEmployerRepository;
        _logger = logger;
    }

    public async Task<DimEmployer> CreateEmployerAsync(string employerName, bool isPublic)
    {
        var allEmployers = (await _dimEmployerRepository.GetAllEmployersAsync()).ToList();
        int newEmployerId = allEmployers.Any() ? allEmployers.Max(e => e.EmployerId) + 1 : 1;
        DimEmployerValidator.ValidateParameters(newEmployerId, employerName, isPublic);
        var employer = new DimEmployer(newEmployerId, employerName, isPublic);
        
        try
        {
            await _dimEmployerRepository.AddEmployerAsync(employer);
            _logger.LogInformation("Created DimEmployer {EmployerId}", newEmployerId);
            return employer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimEmployer {EmployerId}.", employer.EmployerId);
            throw new Exception($"An employer with ID {employer.EmployerId} already exists.");
        }
    }

    public async Task<DimEmployer> GetEmployerByIdAsync(int employerId)
    {
        try
        {
            return await _dimEmployerRepository.GetEmployerByIdAsync(employerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Employer {EmployerId} not found", employerId);
            throw new Exception($"Employer with ID {employerId} was not found.");
        }
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        var employers = await _dimEmployerRepository.GetAllEmployersAsync();
        _logger.LogInformation("Fetched {Count} Employer records", employers.Count());
        return employers;
    }

    public async Task<DimEmployer> UpdateEmployerAsync(int employerId, string employerName, bool isPublic)
    {
        try
        {
            DimEmployerValidator.ValidateParameters(employerId, employerName, isPublic);
            var existingEmployer = await _dimEmployerRepository.GetEmployerByIdAsync(employerId);
            existingEmployer.EmployerName = employerName;
            existingEmployer.IsPublic = isPublic;
            await _dimEmployerRepository.UpdateEmployerAsync(existingEmployer);
            _logger.LogInformation("Updated DimEmployer {EmployerId}", employerId);
            return existingEmployer;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update - Employer {EmployerId} not found", employerId);
            throw new Exception($"Cannot update: employer {employerId} was not found.");
        }
    }

    public async Task DeleteEmployerAsync(int employerId)
    {
        try
        {
            await _dimEmployerRepository.DeleteEmployerAsync(employerId);
            _logger.LogInformation("Deleted DimEmployer {EmployerId}", employerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete, DimEmployer {EmployerId} not found", employerId);
            throw new Exception($"Cannot delete: employer {employerId} not found.");
        }
    }
}