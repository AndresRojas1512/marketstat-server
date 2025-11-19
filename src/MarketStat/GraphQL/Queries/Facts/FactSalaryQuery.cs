using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;
using MarketStat.Services.Facts.FactSalaryService;

namespace MarketStat.GraphQL.Queries.Facts;

[ExtendObjectType("Query")]
public class FactSalaryQuery
{
    public async Task<FactSalaryDto> GetSalaryById(long id, [Service] IFactSalaryService salaryService,
        [Service] IMapper mapper)
    {
        var domainResult = await salaryService.GetFactSalaryByIdAsync(id);
        return mapper.Map<FactSalaryDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<FactSalaryDto>> GetSalaries(SalaryFilterDto filter,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var domainRequest = mapper.Map<AnalysisFilterRequest>(filter);
        var domainResult = await salaryService.GetFactSalariesByFilterAsync(domainRequest);
        return mapper.Map<IEnumerable<FactSalaryDto>>(domainResult);
    }

    public async Task<List<SalaryDistributionBucketDto>> GetSalaryDistribution(SalaryFilterDto filter,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var domainRequest = mapper.Map<AnalysisFilterRequest>(filter);
        var domainResult = await salaryService.GetSalaryDistributionAsync(domainRequest);
        return mapper.Map<List<SalaryDistributionBucketDto>>(domainResult);
    }

    public async Task<SalarySummaryDto?> GetSalarySummary(SalarySummaryRequestDto request,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var domainRequest = mapper.Map<SalarySummaryRequest>(request);
        var domainResult = await salaryService.GetSalarySummaryAsync(domainRequest);
        return mapper.Map<SalarySummaryDto>(domainResult);
    }

    public async Task<List<SalaryTimeSeriesPointDto>> GetSalaryTimeSeries(SalaryTimeSeriesRequestDto request,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var domainRequest = mapper.Map<TimeSeriesRequest>(request);
        var domainResult = await salaryService.GetSalaryTimeSeriesAsync(domainRequest);
        return mapper.Map<List<SalaryTimeSeriesPointDto>>(domainResult);
    }

    public async Task<IEnumerable<PublicRoleByLocationIndustryDto>> GetPublicRoles(PublicRolesRequestDto request,
        [Service] IFactSalaryService salaryService, [Service] IMapper mapper)
    {
        var domainRequest = mapper.Map<PublicRolesRequest>(request);
        var domainResult = await salaryService.GetPublicRolesAsync(domainRequest);
        return mapper.Map<IEnumerable<PublicRoleByLocationIndustryDto>>(domainResult);
    }
}