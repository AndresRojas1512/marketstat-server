using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;
using MarketStat.Services.Dimensions.DimJobService;

namespace MarketStat.GraphQL.Mutations.Dimensions;

[ExtendObjectType("Mutation")]
public class DimJobMutation
{
    public async Task<DimJobDto> CreateJob(CreateDimJobDto input, [Service] IDimJobService jobService,
        [Service] IMapper mapper)
    {
        var createdDomain = await jobService.CreateJobAsync(input.JobRoleTitle, input.StandardJobRoleTitle,
            input.HierarchyLevelName, input.IndustryFieldId);
        return mapper.Map<DimJobDto>(createdDomain);
    }

    public async Task<DimJobDto> UpdateJob(int id, UpdateDimJobDto input, [Service] IDimJobService jobService,
        [Service] IMapper mapper)
    {
        var updatedDomain = await jobService.UpdateJobAsync(id, input.JobRoleTitle, input.StandardJobRoleTitle,
            input.HierarchyLevelName, input.IndustryFieldId);
        return mapper.Map<DimJobDto>(updatedDomain);
    }

    public async Task<bool> DeleteJob(int id, [Service] IDimJobService jobService)
    {
        await jobService.DeleteJobAsync(id);
        return true;
    }
}