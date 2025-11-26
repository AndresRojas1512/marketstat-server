using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimDate;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimDateDomainConsumer :
    IConsumer<ISubmitDimDateCommand>,
    IConsumer<ISubmitDimDateUpdateCommand>,
    IConsumer<ISubmitDimDateDeleteCommand>
{
    private readonly ILogger<DimDateDomainConsumer> _logger;

    public DimDateDomainConsumer(ILogger<DimDateDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitDimDateCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Date {Date}", msg.FullDate);
        try
        {
            DimDateValidator.ValidateForCreate(msg.FullDate);
            await context.Publish<IPersistDimDateCommand>(new
            {
                msg.FullDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Domain: Validation Failed.");
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimDateUpdateCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Update for DateID {Id}", msg.DateId);
        try
        {
            DimDateValidator.ValidateForUpdate(msg.DateId, msg.FullDate);
            await context.Publish<IPersistDimDateCommand>(new
            {
                msg.DateId,
                msg.FullDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Domain: Update Validation Failed.");
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimDateDeleteCommand> context)
    {
        if (context.Message.DateId <= 0)
        {
            return;
        }
        await context.Publish<IPersistDimDateDeleteCommand>(new
        {
            context.Message.DateId
        });
    }
}