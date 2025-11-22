using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Facts;
using MarketStat.Database.Core.Repositories.Facts;
using MassTransit;

namespace MarketStat.Data.Consumers.Facts;

public class GetFactSalaryConsumer : IConsumer<IGetFactSalaryRequest>
{
    private readonly IFactSalaryRepository _repository;
    private readonly ILogger<GetFactSalaryConsumer> _logger;

    public GetFactSalaryConsumer(IFactSalaryRepository repository, ILogger<GetFactSalaryConsumer> logger)
    {
        _repository = repository;
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
}