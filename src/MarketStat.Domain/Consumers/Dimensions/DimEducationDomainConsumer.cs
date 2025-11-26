using MarketStat.Common.Validators.Dimensions;
using MarketStat.Contracts.Dimensions.DimEducation;
using MassTransit;

namespace MarketStat.Domain.Consumers.Dimensions;

public class DimEducationDomainConsumer :
    IConsumer<ISubmitDimEducationCommand>,
    IConsumer<ISubmitDimEducationUpdateCommand>,
    IConsumer<ISubmitDimEducationDeleteCommand>
{
    private readonly ILogger<DimEducationDomainConsumer> _logger;

    public DimEducationDomainConsumer(ILogger<DimEducationDomainConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubmitDimEducationCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Domain: Validating Education {Code}", msg.SpecialtyCode);
        try
        {
            DimEducationValidator.ValidateForCreate(msg.SpecialtyName, msg.SpecialtyCode, msg.EducationLevelName);
            await context.Publish<IPersistDimEducationCommand>(new 
            {
                msg.SpecialtyName, msg.SpecialtyCode, msg.EducationLevelName 
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimEducationUpdateCommand> context)
    {
        var msg = context.Message;
        try
        {
            DimEducationValidator.ValidateForUpdate(msg.EducationId, msg.SpecialtyName, msg.SpecialtyCode, msg.EducationLevelName);
            await context.Publish<IPersistDimEducationUpdateCommand>(new
            {
                msg.EducationId, msg.SpecialtyName, msg.SpecialtyCode, msg.EducationLevelName
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Domain: Update Validation Failed: {Reason}", ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<ISubmitDimEducationDeleteCommand> context)
    {
        if (context.Message.EducationId <= 0) return;
        await context.Publish<IPersistDimEducationDeleteCommand>(new
        {
            context.Message.EducationId 
        });
    }
}