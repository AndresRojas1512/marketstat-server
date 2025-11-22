using MarketStat.Contracts.Sales.Facts;
using MassTransit;

namespace MarketStat.Domain.Consumers.Facts;

public class FactSalaryDomainConsumer : IConsumer<ISubmitFactSalaryCommand>
{
    private readonly ILogger<FactSalaryDomainConsumer> _logger;

    public FactSalaryDomainConsumer(ILogger<FactSalaryDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitFactSalaryCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Processing salary for Employee {EmployeeId}. Amount: {Amount}", msg.EmployeeId,
            msg.SalaryAmount);
        await context.Publish<IPersistFactSalaryCommand>(new
        {
            msg.DateId,
            msg.LocationId,
            msg.EmployerId,
            msg.JobId,
            msg.EmployeeId,
            msg.SalaryAmount
        });
        _logger.LogInformation("Domain: Validation passed. Forwarded to Data Service");
    }
}