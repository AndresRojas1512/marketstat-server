using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Database.Core.Repositories.Dimensions;

namespace MarketStat.Data.Services;

public class FilterResolver
{
    private readonly IDimLocationRepository _locationRepository;
    private readonly IDimJobRepository _jobRepository;
    private readonly IDimIndustryFieldRepository _industryFieldRepository;
    private readonly ILogger<FilterResolver> _logger;

    public FilterResolver(IDimLocationRepository locationRepository, IDimJobRepository jobRepository,
        IDimIndustryFieldRepository industryFieldRepository, ILogger<FilterResolver> logger)
    {
        _locationRepository = locationRepository;
        _jobRepository = jobRepository;
        _industryFieldRepository = industryFieldRepository;
        _logger = logger;
    }

    public async Task<ResolvedSalaryFilter?> ResolveAsync(AnalysisFilterRequest request)
    {
        List<int>? locationIds = null;
        bool locationFilterApplied = false;

        if (!string.IsNullOrEmpty(request.DistrictName) || !string.IsNullOrEmpty(request.OblastName) || !string.IsNullOrEmpty(request.CityName))
        {
            locationFilterApplied = true;
            locationIds = await _locationRepository.GetLocationIdsByFilterAsync(
                request.DistrictName, request.OblastName, request.CityName);
            
            if (locationIds == null || !locationIds.Any())
            {
                _logger.LogInformation("FilterResolver: No locations matched.");
                return null;
            }
        }

        int? resolvedIndustryFieldId = null;
        if (!string.IsNullOrEmpty(request.IndustryFieldName))
        {
            var industry = await _industryFieldRepository.GetIndustryFieldByNameAsync(request.IndustryFieldName);
            if (industry == null)
            {
                 _logger.LogWarning("FilterResolver: Invalid Industry {Name}", request.IndustryFieldName);
                 return null; 
            }
            resolvedIndustryFieldId = industry.IndustryFieldId;
        }

        List<int>? jobIds = null;
        bool jobFilterApplied = false;

        if (!string.IsNullOrEmpty(request.StandardJobRoleTitle) || !string.IsNullOrEmpty(request.HierarchyLevelName) || resolvedIndustryFieldId.HasValue)
        {
            jobFilterApplied = true;
            jobIds = await _jobRepository.GetJobIdsByFilterAsync(
                request.StandardJobRoleTitle, request.HierarchyLevelName, resolvedIndustryFieldId);

            if (jobIds == null || !jobIds.Any())
            {
                _logger.LogInformation("FilterResolver: No jobs matched.");
                return null;
            }
        }

        return new ResolvedSalaryFilter
        {
            LocationIds = locationFilterApplied ? locationIds : null,
            JobIds = jobFilterApplied ? jobIds : null,
            DateStart = request.DateStart,
            DateEnd = request.DateEnd
        };
    }
}