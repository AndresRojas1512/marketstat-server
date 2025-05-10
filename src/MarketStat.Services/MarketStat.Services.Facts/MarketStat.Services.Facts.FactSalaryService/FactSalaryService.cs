using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Facts.FactSalaryService;

public class FactSalaryService : IFactSalaryService
{
    private readonly IFactSalaryRepository _factSalaryRepository;
    private readonly ILogger<FactSalaryService> _logger;

    public FactSalaryService(IFactSalaryRepository factSalaryRepository, ILogger<FactSalaryService> logger)
    {
        _factSalaryRepository = factSalaryRepository;
        _logger = logger;
    }
    
    public async Task<FactSalary> CreateFactSalaryAsync(int dateId, int cityId, int employerId, int jobRoleId, 
        int employeeId, decimal salaryAmount, decimal bonusAmount)
    {
        FactSalaryValidator.ValidateForCreate(dateId, cityId, employerId, jobRoleId, employeeId, salaryAmount, bonusAmount);
        var fact = new FactSalary(0, dateId, cityId, employerId, jobRoleId, employeeId, salaryAmount, bonusAmount);

        try
        {
            await _factSalaryRepository.AddFactSalaryAsync(fact);
            _logger.LogInformation("Created FactSalary {FactId}", fact.SalaryFactId);
            return fact;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "FK not found when creating FactSalary");
            throw;
        }
    }
    
    public async Task<FactSalary> GetFactSalaryByIdAsync(int salaryFactId)
    {
        try
        {
            return await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "FactSalary {FactId} not found", salaryFactId);
            throw;
        }
    }
    
    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var list = await _factSalaryRepository.GetAllFactSalariesAsync();
        _logger.LogInformation("Fetched {Count} salary fact records", list.Count());
        return list;
    }
    
    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(FactSalaryFilter filter)
    {
        var list = (await _factSalaryRepository.GetFactSalariesByFilterAsync(filter)).ToList();
        _logger.LogInformation("Fetched {Count} facts by filter {@Filter}", list.Count, filter);
        return list;
    }
    
    public async Task<FactSalary> UpdateFactSalaryAsync(int salaryFactId, int dateId, int cityId, int employerId,
        int jobRoleId, int employeeId, decimal salaryAmount, decimal bonusAmount)
    {
        FactSalaryValidator.ValidateForUpdate(
            salaryFactId, dateId, cityId, employerId, jobRoleId, employeeId, salaryAmount, bonusAmount);

        try
        {
            var existing = await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);
            
            existing.DateId = dateId;
            existing.CityId = cityId;
            existing.EmployerId = employerId;
            existing.JobRoleId = jobRoleId;
            existing.EmployeeId = employeeId;
            existing.SalaryAmount = salaryAmount;
            existing.BonusAmount = bonusAmount;

            await _factSalaryRepository.UpdateFactSalaryAsync(existing);
            _logger.LogInformation("Updated FactSalary {FactId}", salaryFactId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update, FactSalary {FactId} not found", salaryFactId);
            throw;
        }
    }
    
    public async Task DeleteFactSalaryAsync(int salaryFactId)
    {
        try
        {
            await _factSalaryRepository.DeleteFactSalaryByIdAsync(salaryFactId);
            _logger.LogInformation("Deleted FactSalary {FactId}", salaryFactId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete, FactSalary {FactId} not found", salaryFactId);
            throw;
        }
    }

    public async Task<decimal> GetAverageSalaryAsync(FactSalaryFilter filter)
    {
        var facts   = await _factSalaryRepository.GetFactSalariesByFilterAsync(filter);
        var amounts = facts.Select(f => f.SalaryAmount).ToList();
        if (!amounts.Any())
        {
            _logger.LogInformation("No salary records match filter {@Filter}", filter);
            return 0m;
        }
        var avg = amounts.Select(a => (decimal)a).Average();
        _logger.LogInformation("Computed average {Average} for filter {@Filter}", avg, filter);
        return avg;
    }
}