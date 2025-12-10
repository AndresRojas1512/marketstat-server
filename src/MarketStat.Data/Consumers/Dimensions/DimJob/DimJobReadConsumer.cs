using AutoMapper;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;
using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimJob;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimJob;

public class DimJobReadConsumer :
    IConsumer<IGetDimJobRequest>,
    IConsumer<IGetAllDimJobsRequest>,
    IConsumer<IGetStandardJobRolesRequest>,
    IConsumer<IGetHierarchyLevelsRequest>
{
    private readonly IDimJobRepository _repository;
    private readonly IMapper _mapper;

    public DimJobReadConsumer(IDimJobRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<IGetDimJobRequest> context)
    {
        try
        {
            var job = await _repository.GetJobByIdAsync(context.Message.JobId);
            await context.RespondAsync<IGetDimJobResponse>(new
            {
                job.JobId, job.JobRoleTitle, job.StandardJobRoleTitle, job.HierarchyLevelName, job.IndustryFieldId,
                IndustryField = _mapper.Map<DimIndustryFieldDto>(job.IndustryField)
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IDimJobNotFoundResponse>(new { context.Message.JobId });
        }
    }

    public async Task Consume(ConsumeContext<IGetAllDimJobsRequest> context)
    {
        var list = await _repository.GetAllJobsAsync();
        var responseList = list.Select(j => new
        {
            j.JobId, j.JobRoleTitle, j.StandardJobRoleTitle, j.HierarchyLevelName, j.IndustryFieldId,
            IndustryField = _mapper.Map<DimIndustryFieldDto>(j.IndustryField)
        }).ToList();
        await context.RespondAsync<IGetAllDimJobsResponse>(new { Jobs = responseList });
    }

    public async Task Consume(ConsumeContext<IGetStandardJobRolesRequest> context)
    {
        var roles = await _repository.GetDistinctStandardJobRolesAsync(context.Message.IndustryFieldId);
        await context.RespondAsync<IGetStandardJobRolesResponse>(new { Roles = roles.ToList() });
    }

    public async Task Consume(ConsumeContext<IGetHierarchyLevelsRequest> context)
    {
        var levels = await _repository.GetDistinctHierarchyLevelsAsync(context.Message.IndustryFieldId, context.Message.StandardJobRoleTitle);
        await context.RespondAsync<IGetHierarchyLevelsResponse>(new { Levels = levels.ToList() });
    }
}