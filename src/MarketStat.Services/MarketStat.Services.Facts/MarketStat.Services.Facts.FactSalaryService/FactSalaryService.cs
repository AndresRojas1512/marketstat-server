using System.Data;
using System.Text.Json;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimCityService;
using MarketStat.Services.Dimensions.DimFederalDistrictService;
using MarketStat.Services.Dimensions.DimHierarchyLevelService;
using MarketStat.Services.Dimensions.DimIndustryFieldService;
using MarketStat.Services.Dimensions.DimOblastService;
using MarketStat.Services.Dimensions.DimStandardJobRoleService;
using Npgsql;


namespace MarketStat.Services.Facts.FactSalaryService;

public class FactSalaryService : IFactSalaryService
{
    private readonly IFactSalaryRepository _factSalaryRepository;
    private readonly ILogger<FactSalaryService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly IDimCityService _dimCityService;
    private readonly IDimOblastService _dimOblastService;
    private readonly IDimFederalDistrictService _dimFederalDistrictService;
    private readonly IDimIndustryFieldService _dimIndustryFieldService;
    private readonly IDimStandardJobRoleService _dimStandardJobRoleService;
    private readonly IDimHierarchyLevelService _dimHierarchyLevelService;
    
    private readonly IMapper _mapper;
    private readonly MarketStatDbContext _dbContext;

    private const string PermanentStagingTableName = "marketstat.api_fact_uploads_staging";

    public FactSalaryService(
        IFactSalaryRepository factSalaryRepository,
        IMapper mapper,
        ILogger<FactSalaryService> logger,
        IDimCityService dimCityService,
        IDimOblastService dimOblastService,
        IDimFederalDistrictService dimFederalDistrictService,
        IDimIndustryFieldService dimIndustryFieldService,
        IDimStandardJobRoleService dimStandardJobRoleService,
        IDimHierarchyLevelService dimHierarchyLevelService,
        MarketStatDbContext dbContext)
    {
        _factSalaryRepository = factSalaryRepository ?? throw new ArgumentNullException(nameof(factSalaryRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dimCityService = dimCityService ?? throw new ArgumentNullException(nameof(dimCityService));
        _dimOblastService = dimOblastService ?? throw new ArgumentNullException(nameof(dimOblastService));
        _dimFederalDistrictService = dimFederalDistrictService ?? throw new ArgumentNullException(nameof(dimFederalDistrictService));
        _dimIndustryFieldService = dimIndustryFieldService ?? throw new ArgumentNullException(nameof(dimIndustryFieldService));
        _dimStandardJobRoleService = dimStandardJobRoleService ?? throw new ArgumentNullException(nameof(dimStandardJobRoleService));
        _dimHierarchyLevelService = dimHierarchyLevelService ?? throw new ArgumentNullException(nameof(dimHierarchyLevelService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
        _logger.LogInformation("Service: Validating filters for salary search: {@FilterDto}", filterDto);
        
        try
        {
            if (filterDto.IndustryFieldId.HasValue) await _dimIndustryFieldService.GetIndustryFieldByIdAsync(filterDto.IndustryFieldId.Value);
            if (filterDto.StandardJobRoleId.HasValue) await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(filterDto.StandardJobRoleId.Value);
            if (filterDto.HierarchyLevelId.HasValue) await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(filterDto.HierarchyLevelId.Value);
            if (filterDto.DistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(filterDto.DistrictId.Value);
            if (filterDto.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(filterDto.OblastId.Value);
            if (filterDto.CityId.HasValue) await _dimCityService.GetCityByIdAsync(filterDto.CityId.Value);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid filter ID provided. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }

        _logger.LogInformation("Service: All filter IDs are valid. Fetching salary facts from repository.");
        var list = await _factSalaryRepository.GetFactSalariesByFilterAsync(filterDto);
        _logger.LogInformation("Service: Fetched {Count} facts by filter {@FilterDto}", list.Count(), filterDto);
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
    
    // Auth analytical endpoints
    public async Task<BenchmarkDataDto?> GetBenchmarkingReportAsync(BenchmarkQueryDto filters)
    {
        _logger.LogInformation("Service: Validating filters for benchmark report: {@Filters}", filters);

        try
        {
            if (filters.IndustryFieldId.HasValue) await _dimIndustryFieldService.GetIndustryFieldByIdAsync(filters.IndustryFieldId.Value);
            if (filters.StandardJobRoleId.HasValue) await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(filters.StandardJobRoleId.Value);
            if (filters.HierarchyLevelId.HasValue) await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(filters.HierarchyLevelId.Value);
            if (filters.DistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(filters.DistrictId.Value);
            if (filters.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(filters.OblastId.Value);
            if (filters.CityId.HasValue) await _dimCityService.GetCityByIdAsync(filters.CityId.Value);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid filter ID provided for benchmark report. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }

        if (filters.TargetPercentile < 0 || filters.TargetPercentile > 100)
        {
            _logger.LogWarning("Invalid TargetPercentile provided for benchmark report: {TargetPercentile}", filters.TargetPercentile);
            throw new ArgumentException("The target percentile must be between 0 and 100.", nameof(filters.TargetPercentile));
        }

        _logger.LogInformation("Service: All filters are valid. Fetching benchmark report from repository.");
        string? jsonResult = await _factSalaryRepository.GetBenchmarkingReportJsonAsync(filters);

        _logger.LogInformation("Service: Raw JSON result from repository: {JsonResult}", jsonResult);

        if (string.IsNullOrEmpty(jsonResult) || jsonResult.Trim() == "{}" || jsonResult.Trim() == "null")
        {
            _logger.LogWarning("Service: Benchmark data function returned null or effectively empty JSON for filters: {@Filters}. Raw JSON: {RawJson}", filters, jsonResult);
            return new BenchmarkDataDto();
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
                _logger.LogWarning("Service: Deserialized C# BenchmarkDataDto is null from JSON: {RawJson}. Filters: {@Filters}", jsonResult, filters);
                return new BenchmarkDataDto();
            }

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
        _logger.LogInformation("Service: Validating filters for salary distribution: {@Filters}", filters);

        try
        {
            if (filters.IndustryFieldId.HasValue) await _dimIndustryFieldService.GetIndustryFieldByIdAsync(filters.IndustryFieldId.Value);
            if (filters.StandardJobRoleId.HasValue) await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(filters.StandardJobRoleId.Value);
            if (filters.HierarchyLevelId.HasValue) await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(filters.HierarchyLevelId.Value);
            if (filters.DistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(filters.DistrictId.Value);
            if (filters.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(filters.OblastId.Value);
            if (filters.CityId.HasValue) await _dimCityService.GetCityByIdAsync(filters.CityId.Value);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid filter ID provided for salary distribution. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }

        _logger.LogInformation("Service: All filter IDs are valid. Fetching salary distribution from repository.");
        var result = await _factSalaryRepository.GetSalaryDistributionAsync(filters);
        _logger.LogInformation("Service: Fetched {Count} distribution buckets for filters: {@Filters}", result.Count, filters);
        return result;
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        _logger.LogInformation("Service: Validating filters for salary summary: {@Filters} and target percentile: {TargetPercentile}", 
                               filters, targetPercentile);

        if (targetPercentile < 0 || targetPercentile > 100)
        {
            _logger.LogWarning("Invalid targetPercentile provided for salary summary: {TargetPercentile}", targetPercentile);
            throw new ArgumentException("The target percentile must be between 0 and 100.", nameof(targetPercentile));
        }
        
        try
        {
            if (filters.IndustryFieldId.HasValue) await _dimIndustryFieldService.GetIndustryFieldByIdAsync(filters.IndustryFieldId.Value);
            if (filters.StandardJobRoleId.HasValue) await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(filters.StandardJobRoleId.Value);
            if (filters.HierarchyLevelId.HasValue) await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(filters.HierarchyLevelId.Value);
            if (filters.DistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(filters.DistrictId.Value);
            if (filters.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(filters.OblastId.Value);
            if (filters.CityId.HasValue) await _dimCityService.GetCityByIdAsync(filters.CityId.Value);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid filter ID provided for salary summary. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }

        _logger.LogInformation("Service: All filter IDs are valid. Fetching salary summary from repository.");
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
        _logger.LogInformation("Service: Validating filters for salary time series: {@Filters}", filters);
        
        if (periods <= 0)
        {
            _logger.LogWarning("Invalid periods parameter provided for salary time series: {Periods}", periods);
            throw new ArgumentException("The number of periods must be greater than zero.", nameof(periods));
        }
        try
        {
            if (filters.IndustryFieldId.HasValue) await _dimIndustryFieldService.GetIndustryFieldByIdAsync(filters.IndustryFieldId.Value);
            if (filters.StandardJobRoleId.HasValue) await _dimStandardJobRoleService.GetStandardJobRoleByIdAsync(filters.StandardJobRoleId.Value);
            if (filters.HierarchyLevelId.HasValue) await _dimHierarchyLevelService.GetHierarchyLevelByIdAsync(filters.HierarchyLevelId.Value);
            if (filters.DistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(filters.DistrictId.Value);
            if (filters.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(filters.OblastId.Value);
            if (filters.CityId.HasValue) await _dimCityService.GetCityByIdAsync(filters.CityId.Value);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid filter ID provided for salary time series. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }

        _logger.LogInformation("Service: All filter IDs are valid. Fetching salary time series from repository.");
        var result = await _factSalaryRepository.GetSalaryTimeSeriesAsync(filters, granularity, periods);
        _logger.LogInformation("Service: Fetched {Count} time series points for filters: {@Filters}", result.Count, filters);
        return result;
    }
    
    // Public analytical methods
    
    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto)
    {
        _logger.LogInformation(
            "Service: Validating filters for public roles by location/industry: {@QueryDto}", queryDto);

        if (queryDto.IndustryFieldId <= 0)
        {
            _logger.LogWarning("GetPublicRolesByLocationIndustryAsync called with invalid IndustryFieldId in DTO: {IndustryFieldId}", queryDto.IndustryFieldId);
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(queryDto.IndustryFieldId));
        }
        if (queryDto.MinSalaryRecordsForRole < 0) 
        {
            _logger.LogWarning("GetPublicRolesByLocationIndustryAsync called with invalid minSalaryRecordsForRole in DTO: {MinRecs}", queryDto.MinSalaryRecordsForRole);
            throw new ArgumentException("minSalaryRecordsForRole cannot be negative.", nameof(queryDto.MinSalaryRecordsForRole));
        }

        try
        {
            await _dimIndustryFieldService.GetIndustryFieldByIdAsync(queryDto.IndustryFieldId);

            if (queryDto.FederalDistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(queryDto.FederalDistrictId.Value);
            if (queryDto.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(queryDto.OblastId.Value);
            if (queryDto.CityId.HasValue) await _dimCityService.GetCityByIdAsync(queryDto.CityId.Value);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid filter ID provided for public roles search. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }

        try
        {
            _logger.LogInformation("Service: All filter IDs are valid. Fetching public roles from repository.");
            var result = await _factSalaryRepository.GetPublicRolesByLocationIndustryAsync(queryDto);
            _logger.LogInformation("Service: Successfully retrieved {Count} records for public roles by location/industry.", result.Count());
            return result;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Service: Error retrieving public roles by location/industry for DTO: {@QueryDto}", queryDto);
            throw; 
        }
    }
    
    public async Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(
            PublicSalaryByEducationQueryDto queryDto)
    {
        _logger.LogInformation(
            "Service: Getting public salary by education in industry with DTO: {@QueryDto}", queryDto);
        
        if (queryDto.IndustryFieldId <= 0)
        {
            _logger.LogWarning("GetPublicSalaryByEducationInIndustryAsync called with invalid IndustryFieldId in DTO: {IndustryFieldId}", queryDto.IndustryFieldId);
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(queryDto.IndustryFieldId));
        }
        if (queryDto.TopNSpecialties <= 0)
        {
             throw new ArgumentException("TopNSpecialties must be a positive integer.", nameof(queryDto.TopNSpecialties));
        }
        if (queryDto.MinEmployeesPerSpecialty < 0)
        {
             throw new ArgumentException("MinEmployeesPerSpecialty cannot be negative.", nameof(queryDto.MinEmployeesPerSpecialty));
        }
        if (queryDto.MinEmployeesPerLevelInSpecialty < 0)
        {
             throw new ArgumentException("MinEmployeesPerLevelInSpecialty cannot be negative.", nameof(queryDto.MinEmployeesPerLevelInSpecialty));
        }
        
        try
        {
            await _dimIndustryFieldService.GetIndustryFieldByIdAsync(queryDto.IndustryFieldId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Invalid IndustryFieldId provided for public salary by education search. {ErrorMessage}", ex.Message);
            throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
        }
    
        try
        {
            var result = await _factSalaryRepository.GetPublicSalaryByEducationInIndustryAsync(queryDto);
            _logger.LogInformation("Service: Successfully retrieved {Count} records for public salary by education in industry.", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service: Error retrieving public salary by education in industry for DTO: {@QueryDto}", queryDto);
            throw;
        }
    }
    
    public async Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
            PublicTopEmployerRoleSalariesQueryDto queryDto)
    {
        _logger.LogInformation(
            "Service: Getting public top employer role salaries in industry with DTO: {@QueryDto}", queryDto);
    
        if (queryDto.IndustryFieldId <= 0)
        {
            _logger.LogWarning("GetPublicTopEmployerRoleSalariesInIndustryAsync called with invalid IndustryFieldId in DTO: {IndustryFieldId}", queryDto.IndustryFieldId);
            throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(queryDto.IndustryFieldId));
        }
        if (queryDto.TopNEmployers <= 0)
        {
             throw new ArgumentException("TopNEmployers must be a positive integer.", nameof(queryDto.TopNEmployers));
        }
        if (queryDto.TopMRolesPerEmployer <= 0)
        {
             throw new ArgumentException("TopMRolesPerEmployer must be a positive integer.", nameof(queryDto.TopMRolesPerEmployer));
        }
        if (queryDto.MinSalaryRecordsForRoleAtEmployer < 0)
        {
             throw new ArgumentException("MinSalaryRecordsForRoleAtEmployer cannot be negative.", nameof(queryDto.MinSalaryRecordsForRoleAtEmployer));
        }
    
        try
        {
            var result = await _factSalaryRepository.GetPublicTopEmployerRoleSalariesInIndustryAsync(queryDto);
            _logger.LogInformation("Service: Successfully retrieved {Count} records for public top employer role salaries.", result.Count());
            return result;
        }
        catch (Exception ex) // Catch exceptions from repository (which should be ApplicationException wrapping DB errors)
        {
            _logger.LogError(ex, "Service: Error retrieving public top employer role salaries for DTO: {@QueryDto}", queryDto);
            throw;
        }
    }
    
    // ETL tool

    public async Task<EtlProcessingResultDto> ProcessSalaryFactsCsvUploadAsync(IFormFile csvFile)
    {
        _logger.LogInformation("Service: Starting CSV upload processing for file: {FileName}, Size: {FileSize} bytes", csvFile.FileName, csvFile.Length);
    
        // 1. File Validation
        if (csvFile == null || csvFile.Length == 0)
        {
            _logger.LogWarning("Service: No file uploaded or file is empty.");
            return new EtlProcessingResultDto(false, "No file uploaded or file is empty.");
        }
        if (csvFile.Length > 10 * 1024 * 1024)
        {
            _logger.LogWarning("Service: File {FileName} exceeds size limit of 10MB.", csvFile.FileName);
            return new EtlProcessingResultDto(false, "File exceeds maximum allowed size (10MB).");
        }
        if (Path.GetExtension(csvFile.FileName).ToLowerInvariant() != ".csv")
        {
            _logger.LogWarning("Service: Invalid file type for {FileName}. Only CSV files are allowed.", csvFile.FileName);
            return new EtlProcessingResultDto(false, "Invalid file type. Only CSV files are allowed.");
        }
    
        var recordsToStage = new List<StagedSalaryRecordDto>();
        int csvRowsRead = 0;
    
        // 2. Parse CSV
        try
        {
            _logger.LogInformation("Service: Parsing CSV file {FileName}", csvFile.FileName);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null, 
                HeaderValidated = null,   
                TrimOptions = TrimOptions.Trim,
                BadDataFound = context => _logger.LogWarning("Bad data found in CSV at row number {RowNumber} (approx): {RawRecord}. Field: {Field}", 
                                                              context.Context.Parser.RawRow, context.RawRecord, context.Field)
            };
    
            using (var reader = new StreamReader(csvFile.OpenReadStream(), Encoding.UTF8))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<StagedSalaryRecordDtoMap>();
                await foreach (var record in csv.GetRecordsAsync<StagedSalaryRecordDto>())
                {
                    recordsToStage.Add(record);
                    csvRowsRead++;
                }
            }
            _logger.LogInformation("Service: Parsed {CsvRowsRead} records from CSV file {FileName}.", csvRowsRead, csvFile.FileName);
    
            if (csvRowsRead == 0)
            {
                return new EtlProcessingResultDto(false, "CSV file is empty or contains no valid records.") { CsvRowsRead = 0 };
            }
        }
        catch (HeaderValidationException hvex)
        {
             _logger.LogError(hvex, "Service: CSV header validation error for file {FileName}.", csvFile.FileName);
            return new EtlProcessingResultDto(false, $"CSV header error: {hvex.Message}") { CsvRowsRead = csvRowsRead };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service: Error parsing CSV file {FileName}.", csvFile.FileName);
            return new EtlProcessingResultDto(false, $"Error parsing CSV: {ex.Message}") { CsvRowsRead = csvRowsRead };
        }
    
        int insertedCount = 0;
        int skippedCount = 0;
    
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation("Service: Starting database transaction for staging and bulk load.");
            await _factSalaryRepository.TruncateStagingTableAsync(PermanentStagingTableName);
            await _factSalaryRepository.BatchInsertToStagingTableAsync(PermanentStagingTableName, recordsToStage);
            var procedureResult = await _factSalaryRepository.CallBulkLoadFromStagingProcedureAsync(PermanentStagingTableName);
            insertedCount = procedureResult.insertedCount;
            skippedCount = procedureResult.skippedCount;
            
            await transaction.CommitAsync();
            _logger.LogInformation("Service: CSV processing and bulk load completed successfully for {FileName}. {CsvRowsRead} records read, {StagedCount} records staged.", 
                csvFile.FileName, csvRowsRead, recordsToStage.Count);
            return new EtlProcessingResultDto(true, "Salary facts CSV processed successfully.") 
            { 
                CsvRowsRead = csvRowsRead, 
                RowsStaged = recordsToStage.Count,
                FactsInserted = insertedCount,
                RowsSkippedOrFailedInProcedure = skippedCount
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Service: Error during database staging or bulk load procedure for {FileName}. Transaction rolled back.", csvFile.FileName);
            return new EtlProcessingResultDto(false, $"Database operation failed: {ex.Message}") 
            { 
                CsvRowsRead = csvRowsRead, 
                RowsStaged = (ex is ApplicationException && ex.InnerException is NpgsqlException) ? 0 : recordsToStage.Count,
                FactsInserted = -1,
                RowsSkippedOrFailedInProcedure = -1
            };
        }
    }
}
