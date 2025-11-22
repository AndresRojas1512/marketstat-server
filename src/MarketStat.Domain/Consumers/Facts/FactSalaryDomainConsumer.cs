using MarketStat.Contracts.Facts;
using MassTransit;

namespace MarketStat.Domain.Consumers.Facts;

public class FactSalaryDomainConsumer : 
    IConsumer<ISubmitFactSalaryCommand>,
    IConsumer<ISubmitFactSalaryUpdateCommand>,
    IConsumer<ISubmitFactSalaryDeleteCommand>
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
        if (msg.SalaryAmount < 0)
        {
            _logger.LogWarning("Domain: Invalid Salary (Negative). Dropping message.");
            return;
        }
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

    public async Task Consume(ConsumeContext<ISubmitFactSalaryUpdateCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating UPDATE for Salary {Id}", msg.SalaryFactId);
        if (msg.SalaryAmount < 0)
        {
            _logger.LogWarning("Domain: Update rejected. Negative salary.");
            return;
        }
        await context.Publish<IPersistFactSalaryUpdateCommand>(new
        {
            msg.SalaryFactId,
            msg.DateId,
            msg.LocationId,
            msg.EmployerId,
            msg.JobId,
            msg.EmployeeId,
            msg.SalaryAmount
        });
    }

    public async Task Consume(ConsumeContext<ISubmitFactSalaryDeleteCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating DELETE for Salary {Id}", msg.SalaryFactId);
        await context.Publish<IPersistFactSalaryDeleteCommand>(new
        {
            msg.SalaryFactId
        });
    }
}