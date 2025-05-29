using System.Data;
using System.Text.Json;
using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly ILogger<FactSalaryService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public FactSalaryService(IFactSalaryRepository factSalaryRepository, IMapper mapper, ILogger<FactSalaryService> logger)
    {
        _factSalaryRepository = factSalaryRepository ?? throw new ArgumentNullException(nameof(factSalaryRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<FactSalary> CreateFactSalaryAsync(int dateId, int cityId, int employerId, int jobRoleId, int employeeId, decimal salaryAmount, decimal bonusAmount)
    {
        FactSalaryValidator.ValidateForCreate(dateId, cityId, employerId, jobRoleId, employeeId, salaryAmount, bonusAmount);

        var salary = new FactSalary(
            salaryFactId: 0L,
            dateId: dateId,
            cityId: cityId,
            employerId: employerId,
            jobRoleId: jobRoleId,
            employeeId: employeeId,
            salaryAmount: salaryAmount,
            bonusAmount: bonusAmount
        );
        try
        {
            await _factSalaryRepository.AddFactSalaryAsync(salary);
            _logger.LogInformation("Created salary {FactId}", salary.SalaryFactId);
            return salary;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Cannot create salary fact {SalaryFactId}", salary.SalaryFactId);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "FK not found when creating salary {SalaryFactId}", salary.SalaryFactId);
            throw;
        }
    }

    public async Task<FactSalary> GetFactSalaryByIdAsync(long salaryFactId)
    {
        try
        {
            return await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Salary {SalaryFactId} not found", salaryFactId);
            throw;
        }
    }

    public async Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync()
    {
        _logger.LogInformation("Fetching all salary facts.");
        var list = await _factSalaryRepository.GetAllFactSalariesAsync();
        _logger.LogInformation("Fetched {Count} salary fact records", list.Count());
        return list;
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Fetching salary facts by filter: {@FilterDto}", filterDto);
        var list = await _factSalaryRepository.GetFactSalariesByFilterAsync(filterDto);
        _logger.LogInformation("Fetched {Count} facts by filter {@FilterDto}", list.Count(), filterDto);
        return list;
    }

    public async Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int cityId, int employerId, int jobRoleId, int employeeId, decimal salaryAmount, decimal bonusAmount)
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
            _logger.LogInformation("Updated salary {FactId}", salaryFactId);
            return existing;

        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update, FactSalary {FactId} not found (during update attempt)",
                salaryFactId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Cannot update city {SalaryFactId}: duplicate", salaryFactId);
            throw;
        }
    }

    public async Task DeleteFactSalaryAsync(long salaryFactId)
    {
        try
        {
            await _factSalaryRepository.DeleteFactSalaryByIdAsync(salaryFactId);
            _logger.LogInformation("Deleted salary fact {SalaryFactId}", salaryFactId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete salary fact {SalaryFactId}: not found", salaryFactId);
            throw;
        }
    }
    
    public async Task<BenchmarkDataDto?> GetBenchmarkingReportAsync(BenchmarkQueryDto filters)
    {
        _logger.LogInformation("Service: Fetching benchmark report with filters: {@Filters}", filters);
        string? jsonResult = await _factSalaryRepository.GetBenchmarkingReportJsonAsync(filters);

        // Log the raw JSON received from the repository (which comes from the DB function)
        _logger.LogInformation("Service: Raw JSON result from repository: {JsonResult}", jsonResult);

        if (string.IsNullOrEmpty(jsonResult) || jsonResult.Trim() == "{}" || jsonResult.Trim() == "null")
        {
            _logger.LogWarning("Service: Benchmark data function returned null or effectively empty JSON for filters: {@Filters}. Raw JSON: {RawJson}", filters, jsonResult);
            return new BenchmarkDataDto(); // Constructor initializes lists
        }
        try
        {
            BenchmarkDataDto? benchmarkData = JsonSerializer.Deserialize<BenchmarkDataDto>(jsonResult, _jsonSerializerOptions);
            
            if (benchmarkData != null) 
            {
                // Ensure collections are initialized (good practice, though DTO constructor should handle it)
                benchmarkData.SalaryDistribution ??= new List<SalaryDistributionBucketDto>();
                benchmarkData.SalaryTimeSeries ??= new List<SalaryTimeSeriesPointDto>();
            }
            else
            {
                _logger.LogWarning("Service: Deserialized C# BenchmarkDataDto is null from JSON: {RawJson}. Filters: {@Filters}", jsonResult, filters);
                // This case (valid JSON string but deserializes to null DTO) is rare if DTO is simple class/record.
                return new BenchmarkDataDto(); // Return empty DTO
            }

            // Log the C# DTO object *before* it's returned and re-serialized by ASP.NET Core
            // Using LogDebug as this can be verbose. Ensure your logging level allows Debug.
            // You might want to serialize it to JSON here for a comparable log format to the raw string.
            var optionsForLogging = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
            _logger.LogInformation("Service: Deserialized C# BenchmarkDataDto object: {BenchmarkDataObject}", JsonSerializer.Serialize(benchmarkData, optionsForLogging));
            
            _logger.LogInformation("Service: Successfully fetched and deserialized benchmark report for filters: {@Filters}", filters);
            return benchmarkData;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Service: Error deserializing benchmark report JSON. Raw JSON received: {RawJson}. Filters: {@Filters}", jsonResult, filters);
            throw new ApplicationException("Error processing benchmark report results from database: Invalid data format received.", ex);
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
        else
        {
            _logger.LogInformation(
                "Fetched salary summary for filters: {@Filters}, target percentile: {TargetPercentile}. Total count: {TotalCount}",
                filters, targetPercentile, result.TotalCount);
        }
        return result;
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters, TimeGranularity granularity, int periods)
    {
        _logger.LogInformation("Fetching salary time series with filters: {@Filters}, granularity: {Granularity}, periods: {Periods}", filters, granularity, periods);
        var result = await _factSalaryRepository.GetSalaryTimeSeriesAsync(filters, granularity, periods);
        _logger.LogInformation("Fetched {Count} time series points for filters: {@Filters}, granularity: {Granularity}, periods: {Periods}", result.Count, filters, granularity, periods);
        return result;
    }
    
    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(
        int industryFieldId, 
        int? federalDistrictId, 
        int? oblastId, 
        int? cityId, 
        int minSalaryRecordsForRole)
    {
        _logger.LogInformation("Fetching public roles by location and industry: IndustryFieldId={IndustryFieldId}, DistrictId={DistrictId}, OblastId={OblastId}, CityId={CityId}, MinRecords={MinRecords}", 
            industryFieldId, federalDistrictId, oblastId, cityId, minSalaryRecordsForRole);
            
        if (industryFieldId <= 0)
        {
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(industryFieldId));
        }
        if (minSalaryRecordsForRole < 0)
        {
            throw new ArgumentException("MinSalaryRecordsForRole must be non-negative.", nameof(minSalaryRecordsForRole));
        }

        var result = await _factSalaryRepository.GetPublicRolesByLocationIndustryAsync(industryFieldId, federalDistrictId, oblastId, cityId, minSalaryRecordsForRole);
        _logger.LogInformation("Retrieved {Count} public roles for location/industry query.", result.Count());
        return result;
    }
    
    public async Task<IEnumerable<PublicDegreeByIndustryDto>> GetPublicTopDegreesByIndustryAsync(
        int industryFieldId, 
        int topNDegrees, 
        int minEmployeeCountForDegree)
    {
        _logger.LogInformation("Fetching public top degrees by industry: IndustryFieldId={IndustryFieldId}, TopN={TopN}, MinEmployees={MinEmployees}", 
            industryFieldId, topNDegrees, minEmployeeCountForDegree);

        if (industryFieldId <= 0)
        {
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(industryFieldId));
        }
        if (topNDegrees <= 0)
        {
            throw new ArgumentException("TopNDegrees must be a positive integer.", nameof(topNDegrees));
        }
        if (minEmployeeCountForDegree < 0)
        {
            throw new ArgumentException("MinEmployeeCountForDegree must be non-negative.", nameof(minEmployeeCountForDegree));
        }

        var result = await _factSalaryRepository.GetPublicTopDegreesByIndustryAsync(industryFieldId, topNDegrees, minEmployeeCountForDegree);
        _logger.LogInformation("Retrieved {Count} public top degrees for industry query.", result.Count());
        return result;
    }
}