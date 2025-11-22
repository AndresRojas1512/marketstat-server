using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Contracts.Sales.Facts;
using MarketStat.Database.Core.Repositories.Facts;
using MassTransit;

namespace MarketStat.Data.Consumers;

public class FactSalaryDataConsumer : IConsumer<IPersistFactSalaryCommand>
{
    private readonly IFactSalaryRepository _repository;
    private readonly ILogger<FactSalaryDataConsumer> _logger;

    public FactSalaryDataConsumer(IFactSalaryRepository repository, ILogger<FactSalaryDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IPersistFactSalaryCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data Service: Persisting salary for Employee {EmployeeId}...", msg.EmployeeId);

        var factSalary = new FactSalary
        {
            DateId = msg.DateId,
            LocationId = msg.LocationId,
            EmployerId = msg.EmployerId,
            JobId = msg.JobId,
            EmployeeId = msg.EmployeeId,
            SalaryAmount = msg.SalaryAmount
        };

        try
        {
            await _repository.AddFactSalaryAsync(factSalary);
            _logger.LogInformation("Data Service: Successfully saved FactSalary Id {Id}", factSalary.SalaryFactId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data Service: Failed to save salary.");
            throw;
        }
    }
}