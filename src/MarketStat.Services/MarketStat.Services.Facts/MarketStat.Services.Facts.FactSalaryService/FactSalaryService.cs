namespace MarketStat.Services.Facts.FactSalaryService;

using MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Core.Repositories.Facts;
using MarketStat.Services.Facts.FactSalaryService.Validators;
using Microsoft.Extensions.Logging;

public class FactSalaryService : IFactSalaryService
{
    private readonly IFactSalaryRepository _factSalaryRepository;
    private readonly ILogger<FactSalaryService> _logger;
    private readonly IDimLocationRepository _dimLocationRepository;
    private readonly IDimJobRepository _dimJobRepository;
    private readonly IDimIndustryFieldRepository _dimIndustryFieldRepository;

    public FactSalaryService(
        IFactSalaryRepository factSalaryRepository,
        ILogger<FactSalaryService> logger,
        IDimLocationRepository dimLocationRepository,
        IDimJobRepository dimJobRepository,
        IDimIndustryFieldRepository dimIndustryFieldRepository)
    {
        _factSalaryRepository = factSalaryRepository ?? throw new ArgumentNullException(nameof(factSalaryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dimLocationRepository = dimLocationRepository ?? throw new ArgumentNullException(nameof(dimLocationRepository));
        _dimJobRepository = dimJobRepository ?? throw new ArgumentNullException(nameof(dimJobRepository));
        _dimIndustryFieldRepository = dimIndustryFieldRepository ?? throw new ArgumentNullException(nameof(dimIndustryFieldRepository));
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
            salaryAmount: salaryAmount);
        try
        {
            await _factSalaryRepository.AddFactSalaryAsync(salary).ConfigureAwait(false);
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
            return await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId).ConfigureAwait(false);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Salary {SalaryFactId} not found", salaryFactId);
            throw;
        }
    }

    public async Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(AnalysisFilterRequest request)
    {
        _logger.LogInformation(
            "Service: GetFactSalariesByFilterAsync called with user filters: {@FilterDto}",
            request);
        var resolvedFilters = await ResolveFilters(request).ConfigureAwait(false);
        if (resolvedFilters == null)
        {
            _logger.LogInformation(
                "User filters resolved to no matching dimension IDs. Returning empty list. Returning empty list.");
            return Enumerable.Empty<FactSalary>();
        }

        _logger.LogInformation(
            "Service: Calling repository with resolved filter DTO: {@ResolvedFilters}",
            resolvedFilters);
        var list = await _factSalaryRepository.GetFactSalariesByFilterAsync(resolvedFilters).ConfigureAwait(false);
        _logger.LogInformation("Service: Fetched {Count} facts using resolved filters.", list.Count());
        return list;
    }

    public async Task<FactSalary> UpdateFactSalaryAsync(long salaryFactId, int dateId, int locationId, int employerId, int jobId, int employeeId, decimal salaryAmount)
    {
        FactSalaryValidator.ValidateForUpdate(
            salaryFactId, dateId, locationId, employerId, jobId, employeeId, salaryAmount);

        try
        {
            var existing = await _factSalaryRepository.GetFactSalaryByIdAsync(salaryFactId).ConfigureAwait(false);

            existing.DateId = dateId;
            existing.LocationId = locationId;
            existing.EmployerId = employerId;
            existing.JobId = jobId;
            existing.EmployeeId = employeeId;
            existing.SalaryAmount = salaryAmount;

            await _factSalaryRepository.UpdateFactSalaryAsync(existing).ConfigureAwait(false);
            _logger.LogInformation("Updated salary fact {FactId}", salaryFactId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update, FactSalary {FactId} not found.", salaryFactId);
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
            await _factSalaryRepository.DeleteFactSalaryByIdAsync(salaryFactId).ConfigureAwait(false);
            _logger.LogInformation("Deleted salary fact {SalaryFactId}", salaryFactId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete salary fact {SalaryFactId}: not found", salaryFactId);
            throw;
        }
    }

    // Authorized analytical endpoints
    public async Task<List<SalaryDistributionBucket>> GetSalaryDistributionAsync(AnalysisFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("Service: Getting salary distribution with request: {@Request}", request);
        var resolvedFilters = await ResolveFilters(request).ConfigureAwait(false);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning empty distribution.");
            return new List<SalaryDistributionBucket>();
        }

        return await _factSalaryRepository.GetSalaryDistributionAsync(resolvedFilters).ConfigureAwait(false);
    }

    public async Task<SalarySummary?> GetSalarySummaryAsync(SalarySummaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("Service: Getting salary summary with request: {@Request}", request);
        if (request.TargetPercentile < 0 || request.TargetPercentile > 100)
        {
            throw new ArgumentException(
                "Target percentile must be between 0 and 100.",
                nameof(request));
        }

        var resolvedFilters = await ResolveFilters(request).ConfigureAwait(false);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning null summary");
            return null;
        }

        return await _factSalaryRepository.GetSalarySummaryAsync(resolvedFilters, request.TargetPercentile).ConfigureAwait(false);
    }

    public async Task<List<SalaryTimeSeriesPoint>> GetSalaryTimeSeriesAsync(TimeSeriesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation(
            "Service: Getting salary time series with request: {@Request}", request);
        if (request.Periods <= 0)
        {
            throw new ArgumentException("Periods must be greater than zero.", nameof(request));
        }

        var resolvedFilters = await ResolveFilters(request).ConfigureAwait(false);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning empty time series.");
            return new List<SalaryTimeSeriesPoint>();
        }

        return await _factSalaryRepository.GetSalaryTimeSeriesAsync(
            resolvedFilters,
            request.Granularity,
            request.Periods).ConfigureAwait(false);
    }

    // Public analytical methods
    public async Task<IEnumerable<PublicRoleByLocationIndustry>> GetPublicRolesAsync(PublicRolesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("Service: Getting public roles with request: {$Request}", request);
        if (request.MinRecordCount < 0)
        {
            throw new ArgumentException("Minimum record count cannot be negative.", nameof(request));
        }

        var resolvedFilters = await ResolveFilters(request).ConfigureAwait(false);
        if (resolvedFilters == null)
        {
            _logger.LogInformation("Filters resolved to no matching dimension IDs. Returning empty list.");
            return Enumerable.Empty<PublicRoleByLocationIndustry>();
        }

        return await _factSalaryRepository.GetPublicRolesAsync(resolvedFilters, request.MinRecordCount).ConfigureAwait(false);
    }

    // Utils
    private async Task<ResolvedSalaryFilter?> ResolveFilters(AnalysisFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogDebug("Resolving user request: {@Request}", request);
        List<int>? locationIds = null;
        bool locationFilterApplied = false;

        if (!string.IsNullOrEmpty(request.DistrictName) || !string.IsNullOrEmpty(request.OblastName) ||
            !string.IsNullOrEmpty(request.CityName))
        {
            locationFilterApplied = true;
            _logger.LogDebug(
                "Resolving location IDs based on names: District={District}, Oblast={Oblast}, City={City}", request.DistrictName, request.OblastName, request.CityName);
            locationIds =
                await _dimLocationRepository.GetLocationIdsByFilterAsync(request.DistrictName, request.OblastName, request.CityName).ConfigureAwait(false);
            if (locationIds.Count == 0)
            {
                _logger.LogInformation("No locations matched the specified name filters.");
                return null;
            }

            _logger.LogDebug("Resolved {Count} Location IDs from names.", locationIds.Count);
        }

        List<int>? jobIds = null;
        bool jobFilterApplied = false;

        int? resolvedIndustryFieldId = null;
        if (!string.IsNullOrEmpty(request.IndustryFieldName))
        {
            var industry = await _dimIndustryFieldRepository.GetIndustryFieldByNameAsync(request.IndustryFieldName).ConfigureAwait(false);
            if (industry == null)
            {
                _logger.LogWarning("Invalid IndustryFieldName provided: {IndustryName}", request.IndustryFieldName);
                throw new ArgumentException($"Invalid IndustryFieldName provided: {request.IndustryFieldName}");
            }

            resolvedIndustryFieldId = industry.IndustryFieldId;
        }

        if (!string.IsNullOrEmpty(request.StandardJobRoleTitle) || !string.IsNullOrEmpty(request.HierarchyLevelName) ||
            resolvedIndustryFieldId.HasValue)
        {
            jobFilterApplied = true;
            _logger.LogDebug(
                "Resolving job IDs based on criteria: StandardJobRole={StdJob}, Hierarchy={Hierarchy}, IndustryId={IndustryId}",
                request.StandardJobRoleTitle,
                request.HierarchyLevelName,
                resolvedIndustryFieldId);

            jobIds = await _dimJobRepository.GetJobIdsByFilterAsync(
                request.StandardJobRoleTitle,
                request.HierarchyLevelName,
                resolvedIndustryFieldId).ConfigureAwait(false);

            if (jobIds.Count == 0)
            {
                _logger.LogInformation("No jobs matched the specified job filters.");
                return null;
            }
        }

        return new ResolvedSalaryFilter
        {
            LocationIds = locationFilterApplied ? locationIds : null,
            JobIds = jobFilterApplied ? jobIds : null,
            DateStart = request.DateStart,
            DateEnd = request.DateEnd,
        };
    }
}
