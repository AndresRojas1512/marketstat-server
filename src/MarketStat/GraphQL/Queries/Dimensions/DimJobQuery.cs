using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;
using MarketStat.Services.Dimensions.DimJobService;

namespace MarketStat.GraphQL.Queries.Dimensions;

[ExtendObjectType("Query")]
public class DimJobQuery
{
    public async Task<DimJobDto> GetJobById(int id, [Service] IDimJobService jobService, [Service] IMapper mapper)
    {
        var domainResult = await jobService.GetJobByIdAsync(id);
        return mapper.Map<DimJobDto>(domainResult);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<DimJobDto>> GetAllJobs([Service] IDimJobService jobService, [Service] IMapper mapper)
    {
        var domainResult = await jobService.GetAllJobsAsync();
        return mapper.Map<IEnumerable<DimJobDto>>(domainResult);
    }

    public async Task<IEnumerable<string>> GetStandardJobRoles(int? industryFieldId,
        [Service] IDimJobService jobService)
    {
        return await jobService.GetDistinctStandardJobRolesAsync(industryFieldId);
    }

    public async Task<IEnumerable<string>> GetHierarchyLevels(int? industryFieldId, string? standardJobRoleTitle,
        [Service] IDimJobService jobService)
    {
        return await jobService.GetDistinctHierarchyLevelsAsync(industryFieldId, standardJobRoleTitle);
    }
}