using MarketStat.Common.Exceptions;
using MarketStat.Contracts.Dimensions.DimJob;
using MarketStat.Database.Core.Repositories.Dimensions;
using MassTransit;

namespace MarketStat.Data.Consumers.Dimensions.DimJob;

public class DimJobDataConsumer :
    IConsumer<IPersistDimJobCommand>,
    IConsumer<IPersistDimJobUpdateCommand>,
    IConsumer<IPersistDimJobDeleteCommand>
{
    private readonly IDimJobRepository _repository;
    private readonly ILogger<DimJobDataConsumer> _logger;

    public DimJobDataConsumer(IDimJobRepository repository, ILogger<DimJobDataConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IPersistDimJobCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Data: Saving Job {Title}...", msg.JobRoleTitle);
        var job = new Common.Core.MarketStat.Common.Core.Dimensions.DimJob(0, msg.JobRoleTitle, msg.StandardJobRoleTitle, msg.HierarchyLevelName, msg.IndustryFieldId);
        try
        {
            await _repository.AddJobAsync(job);
            _logger.LogInformation("Data: Saved Job ID {Id}", job.JobId);
        }
        catch (ConflictException)
        {
            _logger.LogWarning("Data: Duplicate Job.");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimJobUpdateCommand> context)
    {
        var msg = context.Message;
        var job = new Common.Core.MarketStat.Common.Core.Dimensions.DimJob(msg.JobId, msg.JobRoleTitle, msg.StandardJobRoleTitle, msg.HierarchyLevelName, msg.IndustryFieldId);
        try
        {
            await _repository.UpdateJobAsync(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed");
        }
    }

    public async Task Consume(ConsumeContext<IPersistDimJobDeleteCommand> context)
    {
        try
        {
            await _repository.DeleteJobAsync(context.Message.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Failed");
        }
    }
}