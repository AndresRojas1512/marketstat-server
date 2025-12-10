using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimIndustryField;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimIndustryFieldDomainConsumer :
    IConsumer<ISubmitDimIndustryFieldCommand>,
    IConsumer<ISubmitDimIndustryFieldUpdateCommand>,
    IConsumer<ISubmitDimIndustryFieldDeleteCommand>
{
    private readonly ILogger<DimIndustryFieldDomainConsumer> _logger;

    public DimIndustryFieldDomainConsumer(ILogger<DimIndustryFieldDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitDimIndustryFieldCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Industry Field {Code}", msg.IndustryFieldCode);
        try
        {
            DimIndustryFieldValidator.ValidateForCreate(msg.IndustryFieldCode, msg.IndustryFieldName);
            await context.Publish<IPersistDimIndustryFieldCommand>(new
            {
                msg.IndustryFieldCode,
                msg.IndustryFieldName
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimIndustryFieldUpdateCommand> context)
    {
        var msg = context.Message;
        try
        {
            DimIndustryFieldValidator.ValidateForUpdate(msg.IndustryFieldId, msg.IndustryFieldCode,
                msg.IndustryFieldName);
            await context.Publish<IPersistDimIndustryFieldUpdateCommand>(new
            {
                msg.IndustryFieldId,
                msg.IndustryFieldCode,
                msg.IndustryFieldName
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Update Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimIndustryFieldDeleteCommand> context)
    {
        if (context.Message.IndustryFieldId <= 0)
        {
            return;
        }
        await context.Publish<IPersistDimIndustryFieldDeleteCommand>(new
        {
            context.Message.IndustryFieldId
        });
    }
}