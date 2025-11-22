using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Facts;
using MarketStat.Database.Core.Repositories.Facts;
using MassTransit;

namespace MarketStat.Data.Consumers.Facts;

public class FactSalaryDataConsumer : 
    IConsumer<IPersistFactSalaryCommand>,
    IConsumer<IPersistFactSalaryUpdateCommand>,
    IConsumer<IPersistFactSalaryDeleteCommand>
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

    public async Task Consume(ConsumeContext<IPersistFactSalaryUpdateCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Updating Salary Fact {Id}...", msg.SalaryFactId);
        var factSalary = new FactSalary
        {
            SalaryFactId = msg.SalaryFactId,
            DateId = msg.DateId,
            LocationId = msg.LocationId,
            EmployerId = msg.EmployerId,
            JobId = msg.JobId,
            EmployeeId = msg.EmployeeId,
            SalaryAmount = msg.SalaryAmount
        };
        try
        {
            await _repository.UpdateFactSalaryAsync(factSalary);
            _logger.LogInformation("Data: Update successful.");
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Data: Update failed. ID {Id} not found.", msg.SalaryFactId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data: Update failed.");
            throw;
        }
    }

    public async Task Consume(ConsumeContext<IPersistFactSalaryDeleteCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Deleting Salary Fact {Id}...", msg.SalaryFactId);
        try
        {
            await _repository.DeleteFactSalaryByIdAsync(msg.SalaryFactId);
            _logger.LogInformation("Data: Delete successful.");
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Data: Delete failed. ID {Id} not found.", msg.SalaryFactId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data: Delete failed.");
            throw;
        }
    }
}