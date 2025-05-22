using System.Data;
using System.Text.Json;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Facts.FactSalaryService;

public class FactSalaryService : IFactSalaryService
{
    private readonly IFactSalaryRepository _factSalaryRepository;
    private readonly ILogger<FactSalaryService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public FactSalaryService(IFactSalaryRepository factSalaryRepository, ILogger<FactSalaryService> logger)
    {
        _factSalaryRepository = factSalaryRepository ?? throw new ArgumentNullException(nameof(factSalaryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
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
            _logger.LogError(ex, "FK not found when creating FactSalary for DateId: {DateId}, CityId: {CityId}, etc.", dateId, cityId);
            throw;
        }
    }

    public async Task<FactSalary?> GetFactSalaryByIdAsync(long salaryFactId)
    {
        try
        {
             return await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("FactSalary {FactId} not found, returning null.", salaryFactId);
            return null;
        }
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        var list = await _factSalaryRepository.GetAllFactSalariesAsync();
        _logger.LogInformation("Fetched {Count} salary fact records", list.Count());
        return list;
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        var list = (await _factSalaryRepository.GetFactSalariesByFilterAsync(filterDto)).ToList();
        _logger.LogInformation("Fetched {Count} facts by filter {@Filter}", list.Count, filterDto);
        return list;
    }

    public async Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int cityId, int employerId,
        int jobRoleId, int employeeId, decimal salaryAmount, decimal bonusAmount)
    {
        FactSalaryValidator.ValidateForUpdate(
            salaryFactId, dateId, cityId, employerId, jobRoleId, employeeId, salaryAmount, bonusAmount);

        var existing = await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);
        if (existing == null) throw new NotFoundException($"FactSalary with ID {salaryFactId} not found for update.");


        existing.DateId = dateId;
        existing.CityId = cityId;
        existing.EmployerId = employerId;
        existing.JobRoleId = jobRoleId;
        existing.EmployeeId = employeeId;
        existing.SalaryAmount = salaryAmount;
        existing.BonusAmount = bonusAmount;

        try
        {
            await _factSalaryRepository.UpdateFactSalaryAsync(existing);
            _logger.LogInformation("Updated FactSalary {FactId}", salaryFactId);
            return existing;
        }
        catch (NotFoundException ex)
        {
             _logger.LogWarning(ex, "Cannot update, FactSalary {FactId} not found (during update attempt)", salaryFactId);
            throw;
        }
    }

    public async Task DeleteFactSalaryAsync(int salaryFactId)
    {
        await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);
        await _factSalaryRepository.DeleteFactSalaryByIdAsync(salaryFactId);
        _logger.LogInformation("Deleted FactSalary {FactId}", salaryFactId);
    }
    
    public async Task<BenchmarkDataDto?> GetBenchmarkingReportAsync(BenchmarkQueryDto filters)
    {
        _logger.LogInformation("Fetching benchmark report with filters: {@Filters}", filters);
        string? jsonResult = await _factSalaryRepository.GetBenchmarkingReportJsonAsync(filters);

        if (string.IsNullOrEmpty(jsonResult) || jsonResult.Trim() == "{}" || jsonResult.Trim() == "null")
        {
            _logger.LogWarning("Benchmark data function returned null or effectively empty JSON for filters: {@Filters}. JSON: {JsonResult}", filters, jsonResult);
            return new BenchmarkDataDto
            {
                SalaryDistribution = new List<SalaryDistributionBucketDto>(),
                SalarySummary = null,
                SalaryTimeSeries = new List<SalaryTimeSeriesPointDto>()
            };
        }

        try
        {
            BenchmarkDataDto? benchmarkData = JsonSerializer.Deserialize<BenchmarkDataDto>(jsonResult, _jsonSerializerOptions);
            
            if (benchmarkData != null) 
            {
                benchmarkData.SalaryDistribution ??= new List<SalaryDistributionBucketDto>();
                benchmarkData.SalaryTimeSeries ??= new List<SalaryTimeSeriesPointDto>();
            }
            else
            {
                _logger.LogWarning("Deserialized benchmarkData is null from JSON: {Json}. Filters: {@Filters}", jsonResult, filters);
                 return new BenchmarkDataDto { };
            }

            _logger.LogInformation("Successfully fetched and deserialized benchmark report for filters: {@Filters}", filters);
            return benchmarkData;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing benchmark report JSON. JSON received: {Json}. Filters: {@Filters}", jsonResult, filters);
            throw new ApplicationException("Error processing benchmark report results from database.", ex);
        }
    }

    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        _logger.LogInformation("Fetching salary distribution with filters: {@Filters}", filters);
        var result = await _factSalaryRepository.GetSalaryDistributionAsync(filters);
        _logger.LogInformation("Fetched {Count} distribution buckets for filters: {@Filters}", result.Count, filters);
        return result;
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        _logger.LogInformation("Fetching salary summary with filters: {@Filters} and target percentile: {TargetPercentile}", filters, targetPercentile);
        var result = await _factSalaryRepository.GetSalarySummaryAsync(filters, targetPercentile);
        if (result == null || result.TotalCount == 0)
        {
            _logger.LogInformation("No salary summary data found for filters: {@Filters}, target percentile: {TargetPercentile}", filters, targetPercentile);
            return null;
        }
        _logger.LogInformation("Fetched salary summary for filters: {@Filters}, target percentile: {TargetPercentile}. Total count: {TotalCount}", filters, targetPercentile, result.TotalCount);
        return result;
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        _logger.LogInformation("Fetching salary time series with filters: {@Filters}, granularity: {Granularity}, periods: {Periods}", filters, granularity, periods);
        var result = await _factSalaryRepository.GetSalaryTimeSeriesAsync(filters, granularity, periods);
        _logger.LogInformation("Fetched {Count} time series points for filters: {@Filters}, granularity: {Granularity}, periods: {Periods}", result.Count, filters, granularity, periods);
        return result;
    }
}