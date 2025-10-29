using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimIndustryFieldService;


namespace MarketStat.Services.Facts.FactSalaryService;

public class FactSalaryService : IFactSalaryService
{
    private readonly IFactSalaryRepository _factSalaryRepository;
    private readonly ILogger<FactSalaryService> _logger;
    private readonly IMapper _mapper;

    private readonly IDimLocationRepository _dimLocationRepository;
    private readonly IDimJobRepository _dimJobRepository;
    private readonly IDimIndustryFieldService _dimIndustryFieldService;
    
    public FactSalaryService(
        IFactSalaryRepository factSalaryRepository,
        IMapper mapper,
        ILogger<FactSalaryService> logger,
        IDimLocationRepository dimLocationRepository,
        IDimJobRepository dimJobRepository,
        IDimIndustryFieldService dimIndustryFieldService)
    {
        _factSalaryRepository = factSalaryRepository ?? throw new ArgumentNullException(nameof(factSalaryRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dimLocationRepository = dimLocationRepository ?? throw new ArgumentNullException(nameof(dimLocationRepository));
        _dimJobRepository = dimJobRepository ?? throw new ArgumentNullException(nameof(dimJobRepository));
        _dimIndustryFieldService = dimIndustryFieldService ?? throw new ArgumentNullException(nameof(dimIndustryFieldService));
    }

    public async Task<FactSalary> CreateFactSalaryAsync(int dateId, int locationId, int employerId, int jobId, int employeeId, decimal salaryAmount)
    {
        FactSalaryValidator.ValidateForCreate(dateId, locationId, employerId, jobId, employeeId, salaryAmount);

        var salary = new FactSalary(
            salaryFactId: 0L,
            dateId: dateId,
            locationId: locationId,
            employerId: employerId,
            jobId: jobId,
            employeeId: employeeId,
            salaryAmount: salaryAmount
        );
        try
        {
            await _factSalaryRepository.AddFactSalaryAsync(salary);
            _logger.LogInformation("Created salary {FactId}", salary.SalaryFactId);
            return salary;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Foreign key violation creating salary fact. Referenced entity might be missing.");
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Cannot create salary fact {SalaryFactId}", salary.SalaryFactId);
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

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(SalaryFilterDto filterDto)
    {
        _logger.LogInformation("Service: GetFactSalariesByFilterAsync called with user filters: {@FilterDto}",
            filterDto);
        var resolvedFilters = await ResolveFilters(filterDto);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("User filters resolved to no matching dimension IDs. Returning empty list.");
            return Enumerable.Empty<FactSalary>();
        }

        _logger.LogInformation("Service: Calling repository with resolved filter DTO: {@ResolvedFilters}",
            resolvedFilters);
        var list = await _factSalaryRepository.GetFactSalariesByFilterAsync(resolvedFilters);
        _logger.LogInformation("Service: Fetched {Count} facts using resolved filters.", list.Count());
        return list;
    }

    public async Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int locationId, int employerId, int jobId, int employeeId, decimal salaryAmount)
    {
        FactSalaryValidator.ValidateForUpdate(
            salaryFactId, dateId, locationId, employerId, jobId, employeeId, salaryAmount);

        try
        {
            var existing = await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId);

            existing.DateId = dateId;
            existing.LocationId = locationId;
            existing.EmployerId = employerId;
            existing.JobId = jobId;
            existing.EmployeeId = employeeId;
            existing.SalaryAmount = salaryAmount;

            await _factSalaryRepository.UpdateFactSalaryAsync(existing);
            _logger.LogInformation("Updated salary fact {FactId}", salaryFactId);
            return existing;

        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update, FactSalary {FactId} not found.",
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

    private async Task<ResolvedSalaryFilterDto> ResolveFilters(SalaryFilterDto userFilters)
    {
        _logger.LogDebug("Resolving user filters: {@UserFilters}", userFilters);
        List<int>? locationIds = null;
        bool locationFilterApplied = false;
        if (userFilters.LocationId.HasValue)
        {
            locationIds = new List<int> { userFilters.LocationId.Value };
            locationFilterApplied = true;
            _logger.LogDebug("Direct LocationId filter applied: {LocationId}", userFilters.LocationId.Value);
        }
        else if (!string.IsNullOrEmpty(userFilters.DistrictName) || !string.IsNullOrEmpty(userFilters.OblastName) ||
                 !string.IsNullOrEmpty(userFilters.CityName))
        {
            locationFilterApplied = true;
            _logger.LogDebug("Resolving location IDs based on names: Distric={District}, Oblast={Oblast}, City={City}",
                userFilters.DistrictName, userFilters.OblastName, userFilters.CityName);
            locationIds = await _dimLocationRepository.GetLocationIdsByFilerAsync(userFilters.DistrictName,
                userFilters.OblastName, userFilters.CityName);
            if (!locationIds.Any())
            {
                _logger.LogInformation("No locations matched the specified name filters.");
                return null;
            }
            _logger.LogDebug("Resolved {Count} Location IDs from names.", locationIds.Count);
        }

        List<int>? jobIds = null;
        bool jobFilterApplied = false;

        if (userFilters.JobId.HasValue)
        {
            jobIds = new List<int> { userFilters.JobId.Value };
            jobFilterApplied = true;
            _logger.LogDebug("Direct JobId filter applied: {JobId}", userFilters.JobId.Value);
        }
        else if (!string.IsNullOrEmpty(userFilters.StandardJobRoleTitle) || !string.IsNullOrEmpty(userFilters.HierarchyLevelName) || userFilters.IndustryFieldId.HasValue)
        {
            jobFilterApplied = true;
            _logger.LogDebug(
                "Resolving job IDs based on criteria: StandardJobRole={StdJob}, Hierarchy={Hierarchy}, IndustryId={IndustryId}",
                userFilters.StandardJobRoleTitle, userFilters.HierarchyLevelName, userFilters.IndustryFieldId);
            if (userFilters.IndustryFieldId.HasValue)
            {
                try
                {
                    await _dimIndustryFieldService.GetIndustryFieldByIdAsync(userFilters.IndustryFieldId.Value);
                }
                catch (NotFoundException ex)
                {
                    throw new ArgumentException(
                        $"Invalid IndustryFieldId provided in filter: {userFilters.IndustryFieldId.Value}", ex);
                }
            }
            jobIds = await _dimJobRepository.GetJobIdsByFilterAsync(userFilters.StandardJobRoleTitle,
                userFilters.HierarchyLevelName, userFilters.IndustryFieldId);
            if (!jobIds.Any())
            {
                _logger.LogInformation("No jobs matched the specified job filters.");
                return null;
            }
            _logger.LogDebug("Resolved {Count} Job IDs from criteria.", jobIds.Count);
        }

        return new ResolvedSalaryFilterDto
        {
            LocationIds = locationFilterApplied ? locationIds : null,
            JobIds = jobFilterApplied ? jobIds : null,
            DateStart = userFilters.DateStart,
            DateEnd = userFilters.DateEnd,
        };
    }
    
    // ===============================
    // Authorized analytical endpoints
    // ===============================
    
    public async Task<BenchmarkDataDto?> GetBenchmarkingReportAsync(BenchmarkQueryDto benchmarkFilters)
    {
        _logger.LogInformation("Service: Generating benchmark report with filters: {@Filters}", benchmarkFilters);
        var userFilter = new SalaryFilterDto
        {
            StandardJobRoleTitle = benchmarkFilters.StandardJobRoleTitle,
            HierarchyLevelName = benchmarkFilters.HierarchyLevelName,
            IndustryFieldId = benchmarkFilters.IndustryFieldId,
            DistrictName = benchmarkFilters.DistrictName,
            OblastName = benchmarkFilters.OblastName,
            CityName = benchmarkFilters.CityName,
            DateStart = benchmarkFilters.DateStart,
            DateEnd = benchmarkFilters.DateEnd
        };
        var resolvedFilters = await ResolveFilters(userFilter);
        var emptyResult = new BenchmarkDataDto
            { SalaryDistribution = new(), SalarySummary = null, SalaryTimeSeries = new() };
        if (resolvedFilters == null)
        {
            _logger.LogInformation(
                "Filters resolved to no matching dimension IDs for benchmark. Returning empty report.");
            return emptyResult;
        }

        if (benchmarkFilters.TargetPercentile < 0 || benchmarkFilters.TargetPercentile > 100)
        {
            throw new ArgumentException("Target percentile must be between 0 and 100.",
                nameof(benchmarkFilters.TargetPercentile));
        }

        if (benchmarkFilters.Periods <= 0)
        {
            throw new ArgumentException("Periods must be greater than zero.", nameof(benchmarkFilters.Periods));
        }
        
        _logger.LogInformation("Calling repository analytical methods with resolved filters for benchmark.");
        var salarySummaryTask = _factSalaryRepository.GetSalarySummaryAsync(resolvedFilters, benchmarkFilters.TargetPercentile);
        var salaryDistributionTask = _factSalaryRepository.GetSalaryDistributionAsync(resolvedFilters);
        var salaryTimeSeriesTask = _factSalaryRepository.GetSalaryTimeSeriesAsync(resolvedFilters, benchmarkFilters.Granularity, benchmarkFilters.Periods);

        await Task.WhenAll(salarySummaryTask, salaryDistributionTask, salaryTimeSeriesTask);

        var salarySummary = await salarySummaryTask;
        var salaryDistribution = await salaryDistributionTask;
        var salaryTimeSeries = await salaryTimeSeriesTask;
        
        _logger.LogInformation("Successfully assembled benchmark report.");
        return new BenchmarkDataDto
        {
            SalarySummary = salarySummary,
            SalaryDistribution = salaryDistribution ?? new List<SalaryDistributionBucketDto>(),
            SalaryTimeSeries = salaryTimeSeries ?? new List<SalaryTimeSeriesPointDto>()
        };
    }
    
    // =========================
    // Public analytical methods
    // =========================
        
    // public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesByLocationIndustryAsync(PublicRolesQueryDto queryDto)
    // {
    //     _logger.LogInformation(
    //         "Service: Validating filters for public roles by location/industry: {@QueryDto}", queryDto);
    //
    //     if (queryDto.IndustryFieldId <= 0)
    //     {
    //         _logger.LogWarning("GetPublicRolesByLocationIndustryAsync called with invalid IndustryFieldId in DTO: {IndustryFieldId}", queryDto.IndustryFieldId);
    //         throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(queryDto.IndustryFieldId));
    //     }
    //     if (queryDto.MinSalaryRecordsForRole < 0) 
    //     {
    //         _logger.LogWarning("GetPublicRolesByLocationIndustryAsync called with invalid minSalaryRecordsForRole in DTO: {MinRecs}", queryDto.MinSalaryRecordsForRole);
    //         throw new ArgumentException("minSalaryRecordsForRole cannot be negative.", nameof(queryDto.MinSalaryRecordsForRole));
    //     }
    //
    //     try
    //     {
    //         await _dimIndustryFieldService.GetIndustryFieldByIdAsync(queryDto.IndustryFieldId);
    //
    //         if (queryDto.FederalDistrictId.HasValue) await _dimFederalDistrictService.GetDistrictByIdAsync(queryDto.FederalDistrictId.Value);
    //         if (queryDto.OblastId.HasValue) await _dimOblastService.GetOblastByIdAsync(queryDto.OblastId.Value);
    //         if (queryDto.CityId.HasValue) await _dimCityService.GetCityByIdAsync(queryDto.CityId.Value);
    //     }
    //     catch (NotFoundException ex)
    //     {
    //         _logger.LogWarning("Invalid filter ID provided for public roles search. {ErrorMessage}", ex.Message);
    //         throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
    //     }
    //
    //     try
    //     {
    //         _logger.LogInformation("Service: All filter IDs are valid. Fetching public roles from repository.");
    //         var result = await _factSalaryRepository.GetPublicRolesByLocationIndustryAsync(queryDto);
    //         _logger.LogInformation("Service: Successfully retrieved {Count} records for public roles by location/industry.", result.Count());
    //         return result;
    //     }
    //     catch (Exception ex) 
    //     {
    //         _logger.LogError(ex, "Service: Error retrieving public roles by location/industry for DTO: {@QueryDto}", queryDto);
    //         throw; 
    //     }
    // }
    //
    // public async Task<IEnumerable<PublicSalaryByEducationInIndustryDto>> GetPublicSalaryByEducationInIndustryAsync(
    //         PublicSalaryByEducationQueryDto queryDto)
    // {
    //     _logger.LogInformation(
    //         "Service: Getting public salary by education in industry with DTO: {@QueryDto}", queryDto);
    //     
    //     if (queryDto.IndustryFieldId <= 0)
    //     {
    //         _logger.LogWarning("GetPublicSalaryByEducationInIndustryAsync called with invalid IndustryFieldId in DTO: {IndustryFieldId}", queryDto.IndustryFieldId);
    //         throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(queryDto.IndustryFieldId));
    //     }
    //     if (queryDto.TopNSpecialties <= 0)
    //     {
    //          throw new ArgumentException("TopNSpecialties must be a positive integer.", nameof(queryDto.TopNSpecialties));
    //     }
    //     if (queryDto.MinEmployeesPerSpecialty < 0)
    //     {
    //          throw new ArgumentException("MinEmployeesPerSpecialty cannot be negative.", nameof(queryDto.MinEmployeesPerSpecialty));
    //     }
    //     if (queryDto.MinEmployeesPerLevelInSpecialty < 0)
    //     {
    //          throw new ArgumentException("MinEmployeesPerLevelInSpecialty cannot be negative.", nameof(queryDto.MinEmployeesPerLevelInSpecialty));
    //     }
    //     
    //     try
    //     {
    //         await _dimIndustryFieldService.GetIndustryFieldByIdAsync(queryDto.IndustryFieldId);
    //     }
    //     catch (NotFoundException ex)
    //     {
    //         _logger.LogWarning("Invalid IndustryFieldId provided for public salary by education search. {ErrorMessage}", ex.Message);
    //         throw new ArgumentException($"Invalid filter ID provided. {ex.Message}", ex);
    //     }
    //
    //     try
    //     {
    //         var result = await _factSalaryRepository.GetPublicSalaryByEducationInIndustryAsync(queryDto);
    //         _logger.LogInformation("Service: Successfully retrieved {Count} records for public salary by education in industry.", result.Count());
    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Service: Error retrieving public salary by education in industry for DTO: {@QueryDto}", queryDto);
    //         throw;
    //     }
    // }
    //
    // public async Task<IEnumerable<PublicTopEmployerRoleSalariesInIndustryDto>> GetPublicTopEmployerRoleSalariesInIndustryAsync(
    //         PublicTopEmployerRoleSalariesQueryDto queryDto)
    // {
    //     _logger.LogInformation(
    //         "Service: Getting public top employer role salaries in industry with DTO: {@QueryDto}", queryDto);
    //
    //     if (queryDto.IndustryFieldId <= 0)
    //     {
    //         _logger.LogWarning("GetPublicTopEmployerRoleSalariesInIndustryAsync called with invalid IndustryFieldId in DTO: {IndustryFieldId}", queryDto.IndustryFieldId);
    //         throw new ArgumentException("IndustryFieldId must be a positive integer.", nameof(queryDto.IndustryFieldId));
    //     }
    //     if (queryDto.TopNEmployers <= 0)
    //     {
    //          throw new ArgumentException("TopNEmployers must be a positive integer.", nameof(queryDto.TopNEmployers));
    //     }
    //     if (queryDto.TopMRolesPerEmployer <= 0)
    //     {
    //          throw new ArgumentException("TopMRolesPerEmployer must be a positive integer.", nameof(queryDto.TopMRolesPerEmployer));
    //     }
    //     if (queryDto.MinSalaryRecordsForRoleAtEmployer < 0)
    //     {
    //          throw new ArgumentException("MinSalaryRecordsForRoleAtEmployer cannot be negative.", nameof(queryDto.MinSalaryRecordsForRoleAtEmployer));
    //     }
    //
    //     try
    //     {
    //         var result = await _factSalaryRepository.GetPublicTopEmployerRoleSalariesInIndustryAsync(queryDto);
    //         _logger.LogInformation("Service: Successfully retrieved {Count} records for public top employer role salaries.", result.Count());
    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Service: Error retrieving public top employer role salaries for DTO: {@QueryDto}", queryDto);
    //         throw;
    //     }
    // }
}
