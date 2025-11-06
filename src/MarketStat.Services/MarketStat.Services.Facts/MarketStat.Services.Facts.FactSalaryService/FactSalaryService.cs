using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MarketStat.Common.Enums;
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

    private async Task<ResolvedSalaryFilterDto?> ResolveFilters(SalaryFilterDto userFilters)
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
            locationIds = await _dimLocationRepository.GetLocationIdsByFilterAsync(userFilters.DistrictName,
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
    
    // Authorized analytical endpoints
    
    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistributionAsync(SalaryFilterDto filters)
    {
        _logger.LogInformation("Service: Getting salary distribution with user filters: {@Filters}", filters);
        var resolvedFilters = await ResolveFilters(filters);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning empty distribution.");
            return new List<SalaryDistributionBucketDto>();
        }
        return await _factSalaryRepository.GetSalaryDistributionAsync(resolvedFilters);
    }

    public async Task<SalarySummaryDto?> GetSalarySummaryAsync(SalaryFilterDto filters, int targetPercentile)
    {
        _logger.LogInformation(
            "Service: Getting salary distribution with user filters: {@Filters} and percentile: {Percentile}", filters,
            targetPercentile);
        if (targetPercentile < 0 || targetPercentile > 100)
            throw new ArgumentException("Target percentile must be between 0 and 100.", nameof(targetPercentile));
        var resolvedFilters = await ResolveFilters(filters);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning null summary.");
            return null;
        }
        return await _factSalaryRepository.GetSalarySummaryAsync(resolvedFilters, targetPercentile);
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeriesAsync(SalaryFilterDto filters,
        TimeGranularity granularity, int periods)
    {
        _logger.LogInformation(
            "Service: Getting salary time series with user filters: {@Filters}, Granularity: {Granularity}, Periods: {Periods}",
            filters, granularity, periods);
        if (periods <= 0)
            throw new ArgumentException("Periods must be greater than zero.", nameof(periods));
        var resolvedFilters = await ResolveFilters(filters);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning empty time series.");
            return new List<SalaryTimeSeriesPointDto>();
        }
        return await _factSalaryRepository.GetSalaryTimeSeriesAsync(resolvedFilters, granularity, periods);
    }
    
    // Public analytical methods

    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRolesAsync(SalaryFilterDto userFilters,
        int minRecordCount)
    {
        _logger.LogInformation("Service: Getting public roles with user filters: {$Filters}", userFilters);
        if (minRecordCount < 0)
            throw new ArgumentException("Minimum record count cannot be negative.", nameof(minRecordCount));
        var resolvedFilters = await ResolveFilters(userFilters);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning empty list.");
            return Enumerable.Empty<PublicRoleByLocationIndustryDto>();
        }
        return await _factSalaryRepository.GetPublicRolesAsync(resolvedFilters, minRecordCount);
    }
}
