using AutoMapper;
using MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Facts;
using MarketStat.Data.Services;
using MarketStat.Database.Core.Repositories.Facts;
using MassTransit;

namespace MarketStat.Data.Consumers.Facts;

public class GetFactSalaryConsumer : 
    IConsumer<IGetFactSalaryRequest>,
    IConsumer<IGetFactSalariesByFilterRequest>
{
    private readonly IFactSalaryRepository _repository;
    private readonly FilterResolver _resolver;
    private readonly ILogger<GetFactSalaryConsumer> _logger;
    private readonly IMapper _mapper;

    public GetFactSalaryConsumer(IFactSalaryRepository repository, FilterResolver resolver, IMapper mapper, ILogger<GetFactSalaryConsumer> logger)
    {
        _repository = repository;
        _resolver = resolver;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetFactSalaryRequest> context)
    {
        _logger.LogInformation("Data: Reading FactSalary ID {Id}", context.Message.SalaryFactId);
        try
        {
            var result = await _repository.GetFactSalaryByIdAsync(context.Message.SalaryFactId);
            await context.RespondAsync<IGetFactSalaryResponse>(new
            {
                result.SalaryFactId,
                result.DateId,
                result.LocationId,
                result.EmployerId,
                result.JobId,
                result.EmployeeId,
                result.SalaryAmount
            });
        }
        catch (NotFoundException)
        {
            await context.RespondAsync<IFactSalaryNotFoundResponse>(new
            {
                context.Message.SalaryFactId
            });
        }
    }

    public async Task Consume(ConsumeContext<IGetFactSalariesByFilterRequest> context)
    {
        _logger.LogInformation("Data: Processing Filtered Salaries Request...");
        var domainRequest = _mapper.Map<AnalysisFilterRequest>(context.Message.Filter);
        var resolved = await _resolver.ResolveAsync(domainRequest);

        if (resolved == null)
        {
            await context.RespondAsync<IGetFactSalariesByFilterResponse>(new
            {
                Salaries = new List<FactSalaryDto>()
            });
            return;
        }
        var result = await _repository.GetFactSalariesByFilterAsync(resolved);
        var dtos = _mapper.Map<List<FactSalaryDto>>(result);
        await context.RespondAsync<IGetFactSalariesByFilterResponse>(new
        {
            Salaries = dtos
        });
    }
}