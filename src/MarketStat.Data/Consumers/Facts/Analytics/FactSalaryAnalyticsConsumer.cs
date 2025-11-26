using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Contracts.Facts.Analytics;
using MarketStat.Data.Services;
using MarketStat.Database.Core.Repositories.Facts;
using MassTransit;

namespace MarketStat.Data.Consumers.Facts.Analytics;

public class FactSalaryAnalyticsConsumer : 
    IConsumer<IGetFactSalaryDistributionRequest>,
    IConsumer<IGetFactSalarySummaryRequest>,
    IConsumer<IGetFactSalaryTimeSeriesRequest>,
    IConsumer<IGetPublicRolesRequest>
{
    private readonly IFactSalaryRepository _repository;
    private readonly FilterResolver _resolver;
    private readonly IMapper _mapper;
    private readonly ILogger<FactSalaryAnalyticsConsumer> _logger;

    public FactSalaryAnalyticsConsumer(IFactSalaryRepository repository, FilterResolver resolver, IMapper mapper,
        ILogger<FactSalaryAnalyticsConsumer> logger)
    {
        _repository = repository;
        _resolver = resolver;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetFactSalaryDistributionRequest> context)
    {
        _logger.LogInformation("Data: Processing Distribution Request...");
        var domainRequest = _mapper.Map<AnalysisFilterRequest>(context.Message.Filter);
        var resolved = await _resolver.ResolveAsync(domainRequest);
        if (resolved == null)
        {
            await context.RespondAsync<IGetFactSalaryDistributionResponse>(new
            {
                Buckets = new List<SalaryDistributionBucketDto>()
            });
            return;
        }
        var result = await _repository.GetSalaryDistributionAsync(resolved);
        var dtos = _mapper.Map<List<SalaryDistributionBucketDto>>(result);
        await context.RespondAsync<IGetFactSalaryDistributionResponse>(new
        {
            Buckets = dtos
        });
    }

    public async Task Consume(ConsumeContext<IGetFactSalarySummaryRequest> context)
    {
        _logger.LogInformation("Data: Processing Summary Request...");
        var domainRequest = _mapper.Map<SalarySummaryRequest>(context.Message.Filter);
        var resolved = await _resolver.ResolveAsync(domainRequest);
        if (resolved == null)
        {
            await context.RespondAsync<IGetFactSalarySummaryResponse>(new
            {
                Summary = new SalarySummaryDto()
            });
            return;
        }
        var result = await _repository.GetSalarySummaryAsync(resolved, context.Message.Filter.TargetPercentile);
        var dto = _mapper.Map<SalarySummaryDto>(result);
        await context.RespondAsync<IGetFactSalarySummaryResponse>(new
        {
            Summary = dto
        });
    }

    public async Task Consume(ConsumeContext<IGetFactSalaryTimeSeriesRequest> context)
    {
        _logger.LogInformation("Data: Processing Time Series Request...");
        var domainRequest = _mapper.Map<TimeSeriesRequest>(context.Message.Filter);
        var resolved = await _resolver.ResolveAsync(domainRequest);
        if (resolved == null)
        {
            await context.RespondAsync<IGetFactSalaryTimeSeriesResponse>(new
            {
                Points = new List<SalaryTimeSeriesPointDto>()
            });
            return;
        }

        var result = await _repository.GetSalaryTimeSeriesAsync(resolved, context.Message.Filter.Granularity,
            context.Message.Filter.Periods);
        var dtos = _mapper.Map<List<SalaryTimeSeriesPointDto>>(result);
        await context.RespondAsync<IGetFactSalaryTimeSeriesResponse>(new
        {
            Points = dtos
        });
    }

    public async Task Consume(ConsumeContext<IGetPublicRolesRequest> context)
    {
        _logger.LogInformation("Data: Processing Public Roles Request...");
        var domainRequest = _mapper.Map<AnalysisFilterRequest>(context.Message.Filter);
        var resolved = await _resolver.ResolveAsync(domainRequest);

        if (resolved == null)
        {
            await context.RespondAsync<IGetPublicRolesResponse>(new
            {
                Roles = new List<PublicRoleByLocationIndustryDto>()
            });
            return;
        }

        var result = await _repository.GetPublicRolesAsync(resolved, context.Message.Filter.MinRecordCount);
        var dtos = _mapper.Map<List<PublicRoleByLocationIndustryDto>>(result);
        await context.RespondAsync<IGetPublicRolesResponse>(new
        {
            Roles = dtos
        });
    }
}